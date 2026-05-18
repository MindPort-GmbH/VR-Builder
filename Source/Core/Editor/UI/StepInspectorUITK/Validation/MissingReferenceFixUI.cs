// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Validation
{
    /// <summary>
    /// Detects two flavours of broken <see cref="ProcessSceneReferenceBase"/> and offers
    /// in-place fix buttons:
    ///   1. The referenced object exists but is missing the required component → "Fix it" button
    ///      attaches the required component automatically.
    ///   2. The reference no longer resolves to any object → "Reassign" helpbox prompts the user
    ///      to drop a new object.
    /// </summary>
    internal static class MissingReferenceFixUI
    {
        /// <summary>
        /// Returns null if the reference is fine, otherwise a VisualElement that can be appended
        /// below the reference's drop-target row.
        /// </summary>
        public static VisualElement BuildFor(ProcessSceneReferenceBase reference, Action onFixed)
        {
            if (reference == null || reference.IsEmpty()) return null;
            if (!RuntimeConfigurator.Exists) return null;

            Type valueType = reference.GetReferenceType();

            // Collect every referenced GameObject and split by whether it's missing the component.
            List<GameObject> missingComponent = new List<GameObject>();
            int resolvedCount = 0;

            foreach (Guid guid in reference.Guids)
            {
                IEnumerable<ISceneObject> objs = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid);
                foreach (ISceneObject obj in objs)
                {
                    if (obj?.GameObject == null) continue;
                    resolvedCount++;
                    if (obj.GameObject.GetComponent(valueType) == null)
                    {
                        missingComponent.Add(obj.GameObject);
                    }
                }
            }

            if (resolvedCount == 0)
            {
                // Reference is non-empty but resolves to nothing — broken reference.
                return BuildBrokenReferenceUI(reference);
            }

            if (missingComponent.Count == 0)
            {
                return null;
            }

            return BuildMissingComponentUI(missingComponent, valueType, onFixed);
        }

        private static VisualElement BuildBrokenReferenceUI(ProcessSceneReferenceBase reference)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-missing-ref");

            HelpBox box = new HelpBox(
                "Referenced object is not present in the current scene. Drag a Process Scene Object onto the field above to reassign it.",
                HelpBoxMessageType.Error);
            box.AddToClassList("vrb-missing-ref__broken");
            root.Add(box);

            return root;
        }

        private static VisualElement BuildMissingComponentUI(List<GameObject> missing, Type valueType, Action onFixed)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-missing-ref");

            string warningText = missing.Count == 1
                ? $"'{missing[0].name}' is missing the required '{valueType.Name}' component."
                : $"{missing.Count} referenced objects are missing the required '{valueType.Name}' component.";

            HelpBox box = new HelpBox(warningText, HelpBoxMessageType.Warning);
            box.AddToClassList("vrb-missing-ref__warning");
            root.Add(box);

            string buttonText = missing.Count == 1 ? $"Fix it — add {valueType.Name}" : $"Fix all ({missing.Count})";
            Button fixButton = new Button(() => ApplyFix(missing, valueType, onFixed))
            {
                text = buttonText,
                tooltip = "Attach the required component to every affected object — undoable."
            };
            fixButton.AddToClassList("vrb-missing-ref__fix-button");
            root.Add(fixButton);

            return root;
        }

        private static void ApplyFix(List<GameObject> missing, Type valueType, Action onFixed)
        {
            // Capture the components present before the fix so undo can restore exactly what we added.
            Dictionary<GameObject, Component[]> alreadyAttached = missing.ToDictionary(
                go => go,
                go => go.GetComponents(typeof(Component)));

            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    foreach (GameObject go in missing)
                    {
                        if (go == null) continue;
                        SceneObjectExtensions.SceneObjectAutomaticSetup(go, valueType);
                    }
                    onFixed?.Invoke();
                },
                () =>
                {
                    foreach (GameObject go in missing)
                    {
                        if (go == null) continue;
                        SceneObjectExtensions.UndoSceneObjectAutomaticSetup(go, valueType, alreadyAttached[go]);
                    }
                    onFixed?.Invoke();
                }));
        }
    }
}
