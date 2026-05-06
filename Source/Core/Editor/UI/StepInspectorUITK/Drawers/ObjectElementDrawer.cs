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
    /// Reflection-based default drawer. Walks the object's drawable fields/properties and
    /// composes one child drawer per member.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(object))]
    public class ObjectElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-object");

            if (value == null)
            {
                Label nullLabel = new Label(label?.text ?? "(null)") { tooltip = label?.tooltip };
                nullLabel.AddToClassList("vrb-object__null");
                root.Add(nullLabel);
                return root;
            }

            if (HasVisibleLabel(label))
            {
                Label header = new Label(label.text) { tooltip = label.tooltip };
                header.AddToClassList("vrb-object__header");
                root.Add(header);
            }

            foreach (MemberInfo member in GetMembersToDraw(value))
            {
                MemberInfo memberInfo = member;
                VisualElement memberElement = BuildMember(value, memberInfo, changeCallback);
                if (memberElement != null)
                {
                    root.Add(memberElement);
                }
            }

            return root;
        }

        public override GUIContent GetLabel(MemberInfo memberInfo, object memberOwner)
        {
            return MergeContents(base.GetLabel(memberInfo, memberOwner), GetTypeNameLabel(MemberAccessCache.GetValue(memberOwner, memberInfo), MemberAccessCache.GetDeclaredType(memberInfo)));
        }

        public override GUIContent GetLabel(object value, Type declaredType)
        {
            return MergeContents(base.GetLabel(value, declaredType), GetTypeNameLabel(value, declaredType));
        }

        protected virtual IEnumerable<MemberInfo> GetMembersToDraw(object value)
        {
            return EditorReflectionUtils.GetFieldsAndPropertiesToDraw(value);
        }

        private VisualElement BuildMember(object owner, MemberInfo memberInfo, Action<object> ownerChangedCallback)
        {
            // Members carrying any [MetadataAttribute] are routed through the wrapper drawer
            // so foldable / deletable / help / reorderable behavior gets applied uniformly.
            if (memberInfo.GetAttributes<MetadataAttribute>(true).Any())
            {
                return MetadataWrapperElementDrawer.BuildMemberThroughWrapper(owner, memberInfo, ownerChangedCallback);
            }

            IElementDrawer memberDrawer = ElementDrawerLocator.GetDrawerForMember(memberInfo, owner);
            if (memberDrawer == null)
            {
                return null;
            }

            object memberValue = MemberAccessCache.GetValue(owner, memberInfo);
            GUIContent memberLabel = memberDrawer.GetLabel(memberInfo, owner);

            Action<object> memberChanged = newValue =>
            {
                MemberAccessCache.SetValue(owner, memberInfo, newValue);
                ownerChangedCallback(owner);
            };

            return memberDrawer.CreateElement(memberValue, memberChanged, memberLabel);
        }

        private static bool HasVisibleLabel(GUIContent label)
        {
            return label != null
                   && label != GUIContent.none
                   && (label.image != null || string.IsNullOrEmpty(label.text) == false);
        }

        private static GUIContent MergeContents(GUIContent name, GUIContent typeName)
        {
            GUIContent result = string.IsNullOrEmpty(name?.text)
                ? new GUIContent(typeName?.text ?? string.Empty) { image = name?.image, tooltip = name?.tooltip }
                : new GUIContent(name.text) { image = name.image, tooltip = name.tooltip };

            if (result.image == null && typeName != null)
            {
                result.image = typeName.image;
            }

            if (string.IsNullOrEmpty(result.tooltip) && typeName != null)
            {
                result.tooltip = typeName.tooltip;
            }

            return result;
        }

        private static GUIContent GetTypeNameLabel(object value, Type declaredType)
        {
            Type actualType = value?.GetType() ?? declaredType;
            if (actualType == null)
            {
                return new GUIContent(string.Empty);
            }

            DisplayNameAttribute typeNameAttribute = actualType.GetAttributes<DisplayNameAttribute>(true).FirstOrDefault();
            return typeNameAttribute != null
                ? new GUIContent(typeNameAttribute.Name)
                : new GUIContent(actualType.FullName);
        }
    }
}
