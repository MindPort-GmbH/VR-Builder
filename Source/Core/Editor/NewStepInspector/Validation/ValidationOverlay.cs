// Copyright (c) 2021-2025 MindPort GmbH

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessValidation;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Validation
{
    /// <summary>
    /// Static utility that decorates VisualElements with validation information
    /// (warning icons and tooltips) from the existing IValidationReport system.
    /// </summary>
    public static class ValidationOverlay
    {
        private const string WarningClass = "step-inspector__validation-warning";

        /// <summary>
        /// Checks validation for a specific data member and applies warning styling if needed.
        /// </summary>
        public static void Apply(VisualElement element, IData data, MemberInfo memberInfo)
        {
            if (element == null || data == null || memberInfo == null) return;
            if (EditorConfigurator.Instance.Validation == null) return;
            if (!EditorConfigurator.Instance.Validation.IsAllowedToValidate()) return;

            List<EditorReportEntry> entries = GetEntriesFor(data, memberInfo);
            if (entries != null && entries.Count > 0)
            {
                ApplyWarning(element, entries);
            }
        }

        /// <summary>
        /// Applies warning styling and tooltip to a VisualElement.
        /// </summary>
        public static void ApplyWarning(VisualElement element, List<EditorReportEntry> entries)
        {
            if (element == null || entries == null || entries.Count == 0) return;

            string tooltip = ValidationTooltipGenerator.CreateTooltip(entries);
            element.tooltip = tooltip;
            element.AddToClassList(WarningClass);
        }

        /// <summary>
        /// Creates a warning HelpBox VisualElement with the given validation entries.
        /// </summary>
        public static VisualElement CreateWarningBox(List<EditorReportEntry> entries)
        {
            if (entries == null || entries.Count == 0) return null;

            string message = ValidationTooltipGenerator.CreateTooltip(entries);
            HelpBox helpBox = new HelpBox(message, HelpBoxMessageType.Warning);
            helpBox.style.marginTop = 4;
            helpBox.style.marginBottom = 4;
            return helpBox;
        }

        private static List<EditorReportEntry> GetEntriesFor(IData data, MemberInfo memberInfo)
        {
            if (EditorConfigurator.Instance.Validation?.LastReport != null)
            {
                return EditorConfigurator.Instance.Validation.LastReport.GetEntriesFor(data, memberInfo);
            }

            return new List<EditorReportEntry>();
        }
    }
}
