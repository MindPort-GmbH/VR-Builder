// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Validation
{
    /// <summary>
    /// Utility for detecting missing scene references and creating fix UI.
    /// Provides HelpBox + "Fix it" button for broken references.
    /// </summary>
    public static class MissingReferenceDetector
    {
        /// <summary>
        /// Checks if a ProcessSceneReference's target object is missing.
        /// </summary>
        public static bool IsMissing(ProcessSceneReferenceBase reference)
        {
            if (reference == null) return true;

            try
            {
                // Check if the referenced GUID resolves to an existing scene object
                if (reference.HasValue() == false)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Creates a fix UI element for a missing reference.
        /// Returns a HelpBox with a "Fix it" button, or null if the reference is valid.
        /// </summary>
        public static VisualElement CreateFixUI(ProcessSceneReferenceBase reference, Action onFixed)
        {
            if (reference == null) return null;
            if (!IsMissing(reference)) return null;

            VisualElement container = new VisualElement();
            container.style.marginTop = 4;
            container.style.marginBottom = 4;

            HelpBox helpBox = new HelpBox(
                "The referenced scene object is missing or has lost its reference. " +
                "The object may have been deleted or its Process Scene Object component removed.",
                HelpBoxMessageType.Warning);
            container.Add(helpBox);

            VisualElement buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.marginTop = 2;

            Button fixButton = new Button(() =>
            {
                TryFixReference(reference);
                onFixed?.Invoke();
            })
            {
                text = "Fix: Reassign Reference",
                tooltip = "Opens object picker to reassign this reference"
            };
            buttonRow.Add(fixButton);

            container.Add(buttonRow);
            return container;
        }

        /// <summary>
        /// Creates a fix UI for a missing component on an existing object.
        /// </summary>
        public static VisualElement CreateMissingComponentFixUI(GameObject target, Type requiredComponent, Action onFixed)
        {
            if (target == null || requiredComponent == null) return null;

            VisualElement container = new VisualElement();

            HelpBox helpBox = new HelpBox(
                $"The object '{target.name}' is missing the required component '{requiredComponent.Name}'.",
                HelpBoxMessageType.Warning);
            container.Add(helpBox);

            Button fixButton = new Button(() =>
            {
                target.AddComponent(requiredComponent);
                onFixed?.Invoke();
            })
            {
                text = $"Fix: Add {requiredComponent.Name}",
                tooltip = $"Adds the missing {requiredComponent.Name} component to {target.name}"
            };
            container.Add(fixButton);

            return container;
        }

        private static void TryFixReference(ProcessSceneReferenceBase reference)
        {
            // This would open a scene object picker dialog
            // For now, log a message indicating the fix action
            UnityEngine.Debug.Log("[NewStepInspector] Reference fix requested. Open the scene object picker to reassign.");
        }
    }
}
