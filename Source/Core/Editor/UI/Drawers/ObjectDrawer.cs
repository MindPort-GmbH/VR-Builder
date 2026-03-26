// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessValidation;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Process drawer for object properties. Used when everything else does not fit.
    /// </summary>
    [DefaultProcessDrawer(typeof(object))]
    public class ObjectDrawer : AbstractDrawer
    {
        /// <inheritdoc />
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            Rect nextPosition = new Rect(rect.x, rect.y, rect.width, EditorDrawingHelper.HeaderLineHeight);
            float height = 0;

            if (currentValue == null)
            {
                EditorGUI.LabelField(rect, label);
                height += nextPosition.height;
                rect.height += height;
                return rect;
            }

            if (label != null && label != GUIContent.none && (label.image != null || label.text != null))
            {
                height += DrawLabel(nextPosition, currentValue, changeValueCallback, label);
            }

            foreach (MemberInfo memberInfoToDraw in GetMembersToDraw(currentValue))
            {
                height += EditorDrawingHelper.VerticalSpacing;
                nextPosition.y = rect.y + height;

                MemberInfo closuredMemberInfo = memberInfoToDraw;

                if (closuredMemberInfo.GetAttributes<MetadataAttribute>(true).Any())
                {
                    height += CreateAndDrawMetadataWrapper(nextPosition, currentValue, closuredMemberInfo, changeValueCallback);
                }
                else
                {
                    IProcessDrawer memberDrawer = DrawerLocator.GetDrawerForMember(closuredMemberInfo, currentValue);

                    object memberValue = MemberAccessCache.GetValue(currentValue, closuredMemberInfo);

                    GUIContent displayName = memberDrawer.GetLabel(closuredMemberInfo, currentValue);

                    CheckValidationForValue(currentValue, closuredMemberInfo, displayName);

                    height += memberDrawer.Draw(nextPosition, memberValue, (value) =>
                    {
                        MemberAccessCache.SetValue(currentValue, closuredMemberInfo, value);
                        changeValueCallback(currentValue);
                    }, displayName).height;
                }
            }

            rect.height = height;
            return rect;
        }

        protected virtual void CheckValidationForValue(object currentValue, MemberInfo info, GUIContent label)
        {
            if (currentValue is IData data && EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                List<EditorReportEntry> entries = GetValidationReportsFor(data, info);
                if (entries.Count > 0)
                {
                    AddValidationInformation(label, entries);
                }
            }
        }

        protected virtual GUIContent AddValidationInformation(GUIContent guiContent, List<EditorReportEntry> entries)
        {
            guiContent.image = EditorGUIUtility.IconContent("Warning").image;
            guiContent.tooltip = ValidationTooltipGenerator.CreateTooltip(entries);
            return guiContent;
        }

        protected virtual List<EditorReportEntry> GetValidationReports(object value)
        {
            if (EditorConfigurator.Instance.Validation.LastReport != null)
            {
                if (value is IConditionData conditionData)
                {
                    return EditorConfigurator.Instance.Validation.LastReport.GetEntriesFor(conditionData);
                }

                if (value is IBehaviorData behaviorData)
                {
                    return EditorConfigurator.Instance.Validation.LastReport.GetEntriesFor(behaviorData);
                }
            }

            return new List<EditorReportEntry>();
        }

        protected virtual List<EditorReportEntry> GetValidationReportsFor(IData data, MemberInfo memberInfo)
        {
            if (EditorConfigurator.Instance.Validation.LastReport != null)
            {
                return EditorConfigurator.Instance.Validation.LastReport.GetEntriesFor(data, memberInfo);
            }
            return new List<EditorReportEntry>();
        }

        /// <summary>
        /// Draw a label for an object.
        /// </summary>
        protected virtual float DrawLabel(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (label == GUIContent.none || label == null || (label.image == null && string.IsNullOrEmpty(label.text)))
            {
                return 0;
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
            };

            EditorGUI.LabelField(rect, label, labelStyle);

            return rect.height;
        }

        private float CreateAndDrawMetadataWrapper(Rect rect, object ownerObject, MemberInfo drawnMemberInfo, Action<object> changeValueCallback)
        {
            Type ownerType = ownerObject.GetType();
            bool hasMetadataMember = MemberAccessCache.TryGetMetadataMember(ownerType, out PropertyInfo metadataProperty, out FieldInfo metadataField);
            Metadata ownerObjectMetadata = null;

            if (hasMetadataMember == false)
            {
                throw new MissingFieldException($"No metadata property on object {ownerObject}.");
            }

            if (metadataProperty != null)
            {
                ownerObjectMetadata = (Metadata)MemberAccessCache.GetValue(ownerObject, metadataProperty) ?? new Metadata();
            }
            else if (metadataField != null)
            {
                ownerObjectMetadata = (Metadata)MemberAccessCache.GetValue(ownerObject, metadataField) ?? new Metadata();
            }
            object memberValue = MemberAccessCache.GetValue(ownerObject, drawnMemberInfo);
            IProcessDrawer memberDrawer = DrawerLocator.GetDrawerForMember(drawnMemberInfo, ownerObject);

            MetadataWrapper wrapper = new MetadataWrapper()
            {
                Metadata = ownerObjectMetadata.GetMetadata(drawnMemberInfo),
                ValueDeclaredType = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(drawnMemberInfo),
                Value = memberValue
            };

            Action<object> wrapperChangedCallback = (newValue) =>
            {
                MetadataWrapper newWrapper = (MetadataWrapper)newValue;
                foreach (string key in newWrapper.Metadata.Keys.ToList())
                {
                    wrapper.Metadata[key] = newWrapper.Metadata[key];
                }

                ownerObjectMetadata.Clear();
                foreach (string key in newWrapper.Metadata.Keys)
                {
                    ownerObjectMetadata.SetMetadata(drawnMemberInfo, key, newWrapper.Metadata[key]);
                }

                if (metadataField != null)
                {
                    MemberAccessCache.SetValue(ownerObject, metadataField, ownerObjectMetadata);
                }

                if (metadataProperty != null)
                {
                    MemberAccessCache.SetValue(ownerObject, metadataProperty, ownerObjectMetadata);
                }

                MemberAccessCache.SetValue(ownerObject, drawnMemberInfo, newWrapper.Value);

                changeValueCallback(ownerObject);
            };

            bool isMetadataDirty = false;

            List<MetadataAttribute> declaredAttributes = drawnMemberInfo.GetAttributes<MetadataAttribute>(true).ToList();

            Dictionary<string, object> obsoleteMetadataRemoved = wrapper.Metadata.Keys.ToList().Where(key => declaredAttributes.Any(attribute => attribute.Name == key)).ToDictionary(key => key, key => wrapper.Metadata[key]);

            if (obsoleteMetadataRemoved.Count < wrapper.Metadata.Count)
            {
                wrapper.Metadata = obsoleteMetadataRemoved;
                isMetadataDirty = true;
            }

            foreach (MetadataAttribute metadataAttribute in declaredAttributes)
            {
                if (wrapper.Metadata.ContainsKey(metadataAttribute.Name) == false)
                {
                    wrapper.Metadata[metadataAttribute.Name] = metadataAttribute.GetDefaultMetadata(drawnMemberInfo);
                    isMetadataDirty = true;
                }
                else if (metadataAttribute.IsMetadataValid(wrapper.Metadata[metadataAttribute.Name]) == false)
                {
                    wrapper.Metadata[metadataAttribute.Name] = metadataAttribute.GetDefaultMetadata(drawnMemberInfo);
                    isMetadataDirty = true;
                }
            }

            if (isMetadataDirty)
            {
                wrapperChangedCallback(wrapper);
            }

            IProcessDrawer wrapperDrawer = DrawerLocator.GetDrawerForValue(wrapper, typeof(MetadataWrapper));

            GUIContent displayName = memberDrawer.GetLabel(drawnMemberInfo, ownerObject);

            return wrapperDrawer.Draw(rect, wrapper, wrapperChangedCallback, displayName).height;
        }

        /// <inheritdoc />
        public override GUIContent GetLabel(MemberInfo memberInfo, object memberOwner)
        {
            return MergeGuiContents(base.GetLabel(memberInfo, memberOwner), GetTypeNameLabel(MemberAccessCache.GetValue(memberOwner, memberInfo), MemberAccessCache.GetDeclaredType(memberInfo)));
        }

        /// <inheritdoc />
        public override GUIContent GetLabel(object value, Type declaredType)
        {
            return MergeGuiContents(base.GetLabel(value, declaredType), GetTypeNameLabel(value, declaredType));
        }

        protected virtual IEnumerable<MemberInfo> GetMembersToDraw(object value)
        {
            return EditorReflectionUtils.GetFieldsAndPropertiesToDraw(value);
        }

        private GUIContent MergeGuiContents(GUIContent name, GUIContent typeName)
        {
            GUIContent result;
            if (name == null || string.IsNullOrEmpty(name.text))
            {
                result = new GUIContent(string.Format("{0}", typeName.text))
                {
                    image = name.image,
                    tooltip = name.tooltip,
                };
            }
            else
            {
                result = new GUIContent(string.Format("{0}", name.text));
            }

            if (result.image == null)
            {
                result.image = typeName.image;
            }

            if (result.tooltip == null)
            {
                result.tooltip = typeName.tooltip;
            }

            return new GUIContent(result);
        }

        protected virtual GUIContent GetTypeNameLabel(object value, Type declaredType)
        {
            Type actualType = declaredType;
            if (value != null)
            {
                actualType = value.GetType();
            }

            DisplayNameAttribute typeNameAttribute = actualType.GetAttributes<DisplayNameAttribute>(true).FirstOrDefault();
            if (typeNameAttribute != null)
            {
                return new GUIContent(typeNameAttribute.Name);
            }

            return new GUIContent(actualType.FullName);
        }
    }
}
