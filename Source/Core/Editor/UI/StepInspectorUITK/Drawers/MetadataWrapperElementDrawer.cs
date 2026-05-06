// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Editor.UI.Drawers;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Applies <see cref="MetadataAttribute"/>-driven decorators (Foldable, Deletable, Help / HelpLink,
    /// Reorderable) on top of the inner value drawer. Phase 2 covers the common subset; the remaining
    /// metadata kinds (Separated, ListOf, ExtendableList, ...) are passed through to the inner drawer
    /// as a no-op and added incrementally in later phases.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(MetadataWrapper))]
    internal class MetadataWrapperElementDrawer : ElementDrawer
    {
        private static readonly string FoldableName = typeof(FoldableAttribute).FullName;
        private static readonly string DeletableName = typeof(DeletableAttribute).FullName;
        private static readonly string ReorderableName = "ReorderableElement";
        private static readonly string HelpName = typeof(HelpAttribute).FullName;

        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            MetadataWrapper wrapper = (MetadataWrapper)value;
            return BuildContent(wrapper, changeCallback, label);
        }

        public override GUIContent GetLabel(object value, Type declaredType)
        {
            MetadataWrapper wrapper = value as MetadataWrapper;
            if (wrapper == null)
            {
                return base.GetLabel(value, declaredType);
            }

            IElementDrawer inner = ElementDrawerLocator.GetDrawerForValue(wrapper.Value, wrapper.ValueDeclaredType);
            return inner != null
                ? inner.GetLabel(wrapper.Value, wrapper.ValueDeclaredType)
                : base.GetLabel(wrapper.Value, wrapper.ValueDeclaredType);
        }

        /// <summary>
        /// Entry point used by <see cref="ObjectElementDrawer"/> when it encounters a member
        /// carrying any <see cref="MetadataAttribute"/>. Mirrors
        /// <c>ObjectDrawer.CreateAndDrawMetadataWrapper</c> on the IMGUI side: it normalizes
        /// the metadata bag, builds a <see cref="MetadataWrapper"/>, and routes through the
        /// wrapper drawer so each metadata kind is applied in turn.
        /// </summary>
        public static VisualElement BuildMemberThroughWrapper(object owner, MemberInfo memberInfo, Action<object> ownerChangedCallback)
        {
            Type ownerType = owner.GetType();
            if (!MemberAccessCache.TryGetMetadataMember(ownerType, out PropertyInfo metadataProperty, out FieldInfo metadataField))
            {
                throw new MissingFieldException($"No metadata property on object {owner}.");
            }

            Metadata ownerMetadata = ReadOwnerMetadata(owner, metadataProperty, metadataField) ?? new Metadata();
            object memberValue = MemberAccessCache.GetValue(owner, memberInfo);
            Type declaredType = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(memberInfo);

            MetadataWrapper wrapper = new MetadataWrapper
            {
                Metadata = ownerMetadata.GetMetadata(memberInfo),
                ValueDeclaredType = declaredType,
                Value = memberValue
            };

            Action<object> wrapperChanged = newValue =>
            {
                MetadataWrapper newWrapper = (MetadataWrapper)newValue;

                // Push the wrapped metadata back onto the owner's Metadata bag.
                foreach (string key in newWrapper.Metadata.Keys.ToList())
                {
                    wrapper.Metadata[key] = newWrapper.Metadata[key];
                }

                ownerMetadata.Clear();
                foreach (string key in newWrapper.Metadata.Keys)
                {
                    ownerMetadata.SetMetadata(memberInfo, key, newWrapper.Metadata[key]);
                }

                WriteOwnerMetadata(owner, ownerMetadata, metadataProperty, metadataField);
                MemberAccessCache.SetValue(owner, memberInfo, newWrapper.Value);
                ownerChangedCallback(owner);
            };

            NormalizeMetadata(wrapper, memberInfo, wrapperChanged);

            IElementDrawer memberDrawer = ElementDrawerLocator.GetDrawerForMember(memberInfo, owner);
            GUIContent label = memberDrawer != null
                ? memberDrawer.GetLabel(memberInfo, owner)
                : new GUIContent(memberInfo.Name);

            IElementDrawer wrapperDrawer = ElementDrawerLocator.GetDrawerForValue(wrapper, typeof(MetadataWrapper));
            return wrapperDrawer.CreateElement(wrapper, wrapperChanged, label);
        }

        private VisualElement BuildContent(MetadataWrapper wrapper, Action<object> changeCallback, GUIContent label)
        {
            if (wrapper.Metadata.ContainsKey(DeletableName))
            {
                return DrawDeletable(wrapper, changeCallback, label);
            }

            if (wrapper.Metadata.ContainsKey(HelpName))
            {
                return DrawHelp(wrapper, changeCallback, label);
            }

            if (wrapper.Metadata.ContainsKey(ReorderableName))
            {
                return DrawReorderable(wrapper, changeCallback, label);
            }

            if (wrapper.Metadata.ContainsKey(FoldableName))
            {
                return DrawFoldable(wrapper, changeCallback, label);
            }

            // Unhandled metadata keys (Separated, ListOf, ExtendableList, KeepPopulated,
            // IsBlockingToggle, Menu) are stripped here so the inner drawer still renders.
            // These get full implementations in later phases.
            return DrawInner(wrapper, changeCallback, label);
        }

        private VisualElement DrawDeletable(MetadataWrapper wrapper, Action<object> changeCallback, GUIContent label)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-meta__deletable");
            row.style.flexDirection = FlexDirection.Row;

            VisualElement inner = DrawRecursive(wrapper, DeletableName, changeCallback, label);
            inner.style.flexGrow = 1f;
            row.Add(inner);

            Button deleteButton = new Button(() => DeleteValue(wrapper, changeCallback))
            {
                text = "✕",
                tooltip = "Remove"
            };
            deleteButton.AddToClassList("vrb-meta__delete-button");
            row.Add(deleteButton);

            return row;
        }

        private VisualElement DrawHelp(MetadataWrapper wrapper, Action<object> changeCallback, GUIContent label)
        {
            VisualElement column = new VisualElement();
            column.AddToClassList("vrb-meta__help");

            HelpLinkAttribute helpLink = wrapper.Value?.GetType().GetCustomAttribute<HelpLinkAttribute>();
            if (helpLink != null)
            {
                Button linkButton = new Button(() => Application.OpenURL(helpLink.HelpLink))
                {
                    text = "Help",
                    tooltip = helpLink.HelpLink
                };
                linkButton.AddToClassList("vrb-meta__help-link");
                column.Add(linkButton);
            }

            column.Add(DrawRecursive(wrapper, HelpName, changeCallback, label));
            return column;
        }

        private VisualElement DrawReorderable(MetadataWrapper wrapper, Action<object> changeCallback, GUIContent label)
        {
            // Visual handle only here; the actual drag manipulator + cross-list move land in Phase 6.
            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-meta__reorderable");
            row.style.flexDirection = FlexDirection.Row;

            VisualElement grip = new VisualElement { name = "vrb-grip" };
            grip.AddToClassList("vrb-grip");
            grip.tooltip = "Drag to reorder or move to another list";
            row.Add(grip);

            VisualElement inner = DrawRecursive(wrapper, ReorderableName, changeCallback, label);
            inner.style.flexGrow = 1f;
            row.Add(inner);

            return row;
        }

        private VisualElement DrawFoldable(MetadataWrapper wrapper, Action<object> changeCallback, GUIContent label)
        {
            bool initialOpen = wrapper.Metadata[FoldableName] is bool b ? b : true;

            Foldout foldout = new Foldout
            {
                text = label?.text ?? string.Empty,
                tooltip = label?.tooltip,
                value = initialOpen
            };
            foldout.AddToClassList("vrb-meta__foldable");

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == initialOpen)
                {
                    return;
                }

                wrapper.Metadata[FoldableName] = evt.newValue;
                changeCallback(wrapper);
                initialOpen = evt.newValue;
            });

            VisualElement inner = DrawRecursive(wrapper, FoldableName, newValue =>
            {
                // If the user changed the wrapped value while collapsed (undo/redo), make sure the
                // foldout is open so the change is visible.
                wrapper.Metadata[FoldableName] = true;
                changeCallback(wrapper);
            }, label);

            foldout.Add(inner);
            return foldout;
        }

        private VisualElement DrawInner(MetadataWrapper wrapper, Action<object> changeCallback, GUIContent label)
        {
            IElementDrawer valueDrawer = ElementDrawerLocator.GetDrawerForValue(wrapper.Value, wrapper.ValueDeclaredType);
            if (valueDrawer == null)
            {
                return new Label($"(no drawer for {wrapper.ValueDeclaredType?.Name})");
            }

            Action<object> valueChanged = newValue =>
            {
                wrapper.Value = newValue;
                changeCallback(wrapper);
            };

            return valueDrawer.CreateElement(wrapper.Value, valueChanged, label);
        }

        private VisualElement DrawRecursive(MetadataWrapper wrapper, string consumedKey, Action<object> changeCallback, GUIContent label)
        {
            // More than one metadata entry: temporarily peel off the one we just rendered and recurse.
            if (wrapper.Metadata.Count > 1)
            {
                return DrawWrapperRecursively(wrapper, consumedKey, changeCallback, label);
            }

            return DrawInner(wrapper, changeCallback, label);
        }

        private VisualElement DrawWrapperRecursively(MetadataWrapper parentWrapper, string removedKey, Action<object> changeCallback, GUIContent label)
        {
            if (!parentWrapper.Metadata.TryGetValue(removedKey, out object removedValue))
            {
                return DrawInner(parentWrapper, changeCallback, label);
            }

            parentWrapper.Metadata.Remove(removedKey);

            MetadataWrapper inner = new MetadataWrapper
            {
                Value = parentWrapper.Value,
                ValueDeclaredType = parentWrapper.ValueDeclaredType,
                Metadata = parentWrapper.Metadata
            };

            Action<object> innerChanged = newValue =>
            {
                MetadataWrapper newWrapper = (MetadataWrapper)newValue;
                parentWrapper.Value = newWrapper.Value;

                if (!parentWrapper.Metadata.ContainsKey(removedKey))
                {
                    parentWrapper.Metadata.Add(removedKey, removedValue);
                }

                changeCallback(parentWrapper);
                parentWrapper.Metadata.Remove(removedKey);
            };

            try
            {
                return BuildContent(inner, innerChanged, label);
            }
            finally
            {
                if (!parentWrapper.Metadata.ContainsKey(removedKey))
                {
                    parentWrapper.Metadata.Add(removedKey, removedValue);
                }
            }
        }

        private void DeleteValue(MetadataWrapper wrapper, Action<object> changeCallback)
        {
            object oldValue = wrapper.Value;
            ChangeValue(
                getNewValueCallback: () => { wrapper.Value = null; return wrapper; },
                getOldValueCallback: () => { wrapper.Value = oldValue; return wrapper; },
                assignValueCallback: changeCallback);
        }

        private static Metadata ReadOwnerMetadata(object owner, PropertyInfo property, FieldInfo field)
        {
            if (property != null)
            {
                return MemberAccessCache.GetValue(owner, property) as Metadata;
            }

            if (field != null)
            {
                return MemberAccessCache.GetValue(owner, field) as Metadata;
            }

            return null;
        }

        private static void WriteOwnerMetadata(object owner, Metadata metadata, PropertyInfo property, FieldInfo field)
        {
            if (field != null)
            {
                MemberAccessCache.SetValue(owner, field, metadata);
            }

            if (property != null)
            {
                MemberAccessCache.SetValue(owner, property, metadata);
            }
        }

        private static void NormalizeMetadata(MetadataWrapper wrapper, MemberInfo memberInfo, Action<object> wrapperChanged)
        {
            List<MetadataAttribute> declared = memberInfo.GetAttributes<MetadataAttribute>(true).ToList();
            bool dirty = false;

            // Drop stale keys whose attribute was removed from the member.
            List<string> staleKeys = wrapper.Metadata.Keys
                .Where(key => declared.All(attr => attr.Name != key))
                .ToList();
            if (staleKeys.Count > 0)
            {
                foreach (string key in staleKeys)
                {
                    wrapper.Metadata.Remove(key);
                }
                dirty = true;
            }

            foreach (MetadataAttribute attribute in declared)
            {
                if (!wrapper.Metadata.ContainsKey(attribute.Name))
                {
                    wrapper.Metadata[attribute.Name] = attribute.GetDefaultMetadata(memberInfo);
                    dirty = true;
                }
                else if (!attribute.IsMetadataValid(wrapper.Metadata[attribute.Name]))
                {
                    wrapper.Metadata[attribute.Name] = attribute.GetDefaultMetadata(memberInfo);
                    dirty = true;
                }
            }

            if (dirty)
            {
                wrapperChanged(wrapper);
            }
        }
    }
}
