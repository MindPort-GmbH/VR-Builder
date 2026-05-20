using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessValidation;
using VRBuilder.Core.ProcessValidation;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Validation
{
    /// <summary>
    /// Adds warning / error chrome to inspector elements based on the validation report
    /// from <see cref="EditorConfigurator.Instance.Validation"/>. Each helper is no-op when
    /// validation is disabled or the report has no matching entries, so callers can blanket
    /// invoke them without conditionals.
    /// </summary>
    internal static class ValidationOverlay
    {
        private const string ErrorIcon = "console.erroricon";
        private const string WarningIcon = "console.warnicon";
        private const string InfoIcon = "console.infoicon";

        /// <summary>
        /// Decorates a field VisualElement with a warning / error icon when the report has
        /// entries for the given <paramref name="data"/> + <paramref name="memberInfo"/>.
        /// </summary>
        public static void DecorateMember(VisualElement field, IData data, MemberInfo memberInfo)
        {
            if (field == null || data == null || memberInfo == null) return;
            if (TryGetMemberEntries(data, memberInfo, out List<EditorReportEntry> entries) == false) return;

            ApplyEntries(field, entries);
        }

        /// <summary>
        /// Decorates the header row of a behavior or condition with a warning icon when the
        /// validation report has any entry for the entity's data.
        /// </summary>
        public static void DecorateEntityHeader(VisualElement header, IEntity entity)
        {
            if (header == null || entity == null) return;
            if (TryGetEntityEntries(entity, out List<EditorReportEntry> entries) == false) return;

            Image icon = BuildIcon(entries);
            if (icon == null) return;

            icon.AddToClassList("vrb-validation-icon");
            icon.AddToClassList("vrb-validation-icon--header");
            // Insert right after the caret so the icon shows next to the title.
            header.Insert(2, icon);
        }

        /// <summary>
        /// Returns the icon for the worst entry in the given step's behaviors. Used by
        /// tab titles + panel hosts. Null when no entries.
        /// </summary>
        public static Image BuildBehaviorsTabIcon(IStepData stepData)
        {
            if (TryGetReport(out IValidationReport report) == false) return null;

            List<EditorReportEntry> entries = report.GetBehaviorEntriesFor(stepData);
            return entries != null && entries.Count > 0 ? BuildIcon(entries) : null;
        }

        /// <summary>
        /// Returns the icon for the worst entry across the step's transitions. Null when no entries.
        /// </summary>
        public static Image BuildTransitionsTabIcon(IStepData stepData)
        {
            if (TryGetReport(out IValidationReport report) == false) return null;

            List<EditorReportEntry> entries = report.GetConditionEntriesFor(stepData);
            return entries != null && entries.Count > 0 ? BuildIcon(entries) : null;
        }

        // ───────── internals ─────────

        private static bool TryGetMemberEntries(IData data, MemberInfo memberInfo, out List<EditorReportEntry> entries)
        {
            entries = null;
            if (TryGetReport(out IValidationReport report) == false) return false;

            entries = report.GetEntriesFor(data, memberInfo);
            return entries != null && entries.Count > 0;
        }

        private static bool TryGetEntityEntries(IEntity entity, out List<EditorReportEntry> entries)
        {
            entries = null;
            if (TryGetReport(out IValidationReport report) == false) return false;

            if (entity is IDataOwner owner)
            {
                if (owner.Data is IBehaviorData behaviorData)
                {
                    entries = report.GetEntriesFor(behaviorData);
                }
                else if (owner.Data is IConditionData conditionData)
                {
                    entries = report.GetEntriesFor(conditionData);
                }
            }

            return entries != null && entries.Count > 0;
        }

        private static bool TryGetReport(out IValidationReport report)
        {
            report = null;
            try
            {
                if (EditorConfigurator.Instance?.Validation == null) return false;
                if (EditorConfigurator.Instance.Validation.IsAllowedToValidate() == false) return false;

                report = EditorConfigurator.Instance.Validation.LastReport;
                return report != null;
            }
            catch
            {
                return false;
            }
        }

        private static void ApplyEntries(VisualElement target, List<EditorReportEntry> entries)
        {
            // Wrap the field in a row that has an icon on the right so we don't disturb the
            // field's own internal layout.
            VisualElement parent = target.parent;
            if (parent == null) return;

            int index = parent.IndexOf(target);
            if (index < 0) return;

            Image icon = BuildIcon(entries);
            if (icon == null) return;

            icon.AddToClassList("vrb-validation-icon");
            icon.AddToClassList("vrb-validation-icon--field");

            // If the field already has a sibling icon (re-decoration), remove it first.
            if (index + 1 < parent.childCount && parent[index + 1].ClassListContains("vrb-validation-icon--field"))
            {
                parent.RemoveAt(index + 1);
            }
            parent.Insert(index + 1, icon);
        }

        private static Image BuildIcon(List<EditorReportEntry> entries)
        {
            if (entries == null || entries.Count == 0) return null;

            ValidationErrorLevel worst = entries.Max(entry => entry.ErrorLevel);
            string iconName = worst switch
            {
                ValidationErrorLevel.ERROR => ErrorIcon,
                ValidationErrorLevel.WARNING => WarningIcon,
                _ => InfoIcon,
            };

            UnityEngine.Texture iconTexture = EditorGUIUtility.IconContent(iconName).image;
            if (iconTexture == null) return null;

            Image icon = new Image
            {
                image = iconTexture,
                tooltip = ValidationTooltipGenerator.CreateTooltip(entries)
            };
            return icon;
        }
    }
}
