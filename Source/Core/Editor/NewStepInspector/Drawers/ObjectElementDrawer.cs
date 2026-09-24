// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessValidation;
using VRBuilder.Core.Editor.UI.Drawers;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit port of <see cref="VRBuilder.Core.Editor.UI.Drawers.ObjectDrawer"/>.
    /// The critical auto-drawer that uses reflection to enumerate data members
    /// and recursively delegates to type-specific element drawers.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(object))]
    public class ObjectElementDrawer : ElementDrawer
    {
        /// <inheritdoc />
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            VisualElement container = new VisualElement();
            container.style.marginBottom = 2;

            if (currentValue == null)
            {
                container.Add(new Label(label ?? "null"));
                return container;
            }

            if (!string.IsNullOrEmpty(label))
            {
                Label headerLabel = DrawLabel(label, currentValue);
                if (headerLabel != null)
                {
                    container.Add(headerLabel);
                }
            }

            foreach (MemberInfo memberInfo in GetMembersToDraw(currentValue))
            {
                MemberInfo closuredMemberInfo = memberInfo;

                if (closuredMemberInfo.GetAttributes<MetadataAttribute>(true).Any())
                {
                    // MetadataWrapper handling - skip for now, can be added later
                    continue;
                }

                IElementDrawer memberDrawer = ElementDrawerLocator.GetDrawerForMember(closuredMemberInfo, currentValue);
                if (memberDrawer == null)
                {
                    continue;
                }

                object memberValue = MemberAccessCache.GetValue(currentValue, closuredMemberInfo);
                string displayName = memberDrawer.GetLabel(closuredMemberInfo, currentValue);

                VisualElement element = memberDrawer.CreateElement(memberValue, (newValue) =>
                {
                    MemberAccessCache.SetValue(currentValue, closuredMemberInfo, newValue);
                    changeValueCallback(currentValue);
                }, displayName);

                if (element != null)
                {
                    CheckValidationForValue(currentValue, closuredMemberInfo, element);
                    container.Add(element);
                }
            }

            return container;
        }

        /// <inheritdoc />
        public override string GetLabel(MemberInfo memberInfo, object memberOwner)
        {
            string baseName = base.GetLabel(memberInfo, memberOwner);
            string typeName = GetTypeNameLabel(
                MemberAccessCache.GetValue(memberOwner, memberInfo),
                MemberAccessCache.GetDeclaredType(memberInfo));

            if (string.IsNullOrEmpty(baseName))
            {
                return typeName;
            }

            return baseName;
        }

        /// <inheritdoc />
        public override string GetLabel(object value, Type declaredType)
        {
            string baseName = base.GetLabel(value, declaredType);
            string typeName = GetTypeNameLabel(value, declaredType);

            if (string.IsNullOrEmpty(baseName))
            {
                return typeName;
            }

            return baseName;
        }

        /// <summary>
        /// Returns the members to draw for the given value.
        /// Uses the existing EditorReflectionUtils infrastructure.
        /// </summary>
        protected virtual IEnumerable<MemberInfo> GetMembersToDraw(object value)
        {
            return EditorReflectionUtils.GetFieldsAndPropertiesToDraw(value);
        }

        /// <summary>
        /// Creates a bold header label for the object.
        /// </summary>
        protected virtual Label DrawLabel(string text, object currentValue)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            Label label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.fontSize = 12;
            label.style.marginBottom = 4;
            return label;
        }

        /// <summary>
        /// Checks validation for a specific member and adds warning styling if needed.
        /// </summary>
        protected virtual void CheckValidationForValue(object currentValue, MemberInfo memberInfo, VisualElement element)
        {
            if (currentValue is IData data && EditorConfigurator.Instance.Validation != null
                && EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                List<EditorReportEntry> entries = GetValidationReportsFor(data, memberInfo);
                if (entries.Count > 0)
                {
                    AddValidationInformation(element, entries);
                }
            }
        }

        /// <summary>
        /// Adds a warning icon and tooltip to the element.
        /// </summary>
        protected virtual void AddValidationInformation(VisualElement element, List<EditorReportEntry> entries)
        {
            string tooltip = ValidationTooltipGenerator.CreateTooltip(entries);
            element.tooltip = tooltip;
            element.AddToClassList("step-inspector__validation-warning");
        }

        protected virtual List<EditorReportEntry> GetValidationReports(object value)
        {
            if (EditorConfigurator.Instance.Validation?.LastReport != null)
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
            if (EditorConfigurator.Instance.Validation?.LastReport != null)
            {
                return EditorConfigurator.Instance.Validation.LastReport.GetEntriesFor(data, memberInfo);
            }

            return new List<EditorReportEntry>();
        }

        private string GetTypeNameLabel(object value, Type declaredType)
        {
            Type actualType = declaredType;
            if (value != null)
            {
                actualType = value.GetType();
            }

            DisplayNameAttribute typeNameAttribute = actualType.GetAttributes<DisplayNameAttribute>(true).FirstOrDefault();
            if (typeNameAttribute != null)
            {
                return typeNameAttribute.Name;
            }

            return actualType.Name;
        }
    }
}
