// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.References
{
    /// <summary>
    /// UIToolkit drawer for any <see cref="ProcessSceneReferenceBase"/>.
    /// Renders the label + a drop-target box that accepts a GameObject (or
    /// ProcessSceneObject) from the Hierarchy, plus a "Clear" button.
    /// Full group-edit popup is a Phase 7 follow-up.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(ProcessSceneReferenceBase))]
    internal class ProcessSceneReferenceElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            ProcessSceneReferenceBase reference = value as ProcessSceneReferenceBase;

            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-scene-ref");

            if (label != null && string.IsNullOrEmpty(label.text) == false)
            {
                Label heading = new Label(label.text) { tooltip = label.tooltip };
                heading.AddToClassList("vrb-scene-ref__label");
                root.Add(heading);
            }

            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-scene-ref__row");
            row.style.flexDirection = FlexDirection.Row;

            Label dropBox = new Label(DescribeReference(reference))
            {
                tooltip = "Drop a Process Scene Object here to assign it."
            };
            dropBox.AddToClassList("vrb-scene-ref__drop");
            dropBox.style.flexGrow = 1f;
            row.Add(dropBox);

            Button clear = new Button(() => ClearReference(reference, changeCallback))
            {
                text = "✕",
                tooltip = "Clear the reference"
            };
            clear.AddToClassList("vrb-scene-ref__clear");
            row.Add(clear);

            root.Add(row);

            RegisterDropHandlers(dropBox, reference, changeCallback);

            if (reference != null)
            {
                AppendUnconfiguredWarnings(root, reference);
            }

            return root;
        }

        private void RegisterDropHandlers(VisualElement target, ProcessSceneReferenceBase reference, Action<object> changeCallback)
        {
            target.RegisterCallback<DragEnterEvent>(_ =>
            {
                if (HasDraggedGameObject())
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    target.AddToClassList("vrb-scene-ref__drop--hover");
                }
            });

            target.RegisterCallback<DragUpdatedEvent>(_ =>
            {
                if (HasDraggedGameObject())
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
            });

            target.RegisterCallback<DragLeaveEvent>(_ => target.RemoveFromClassList("vrb-scene-ref__drop--hover"));

            target.RegisterCallback<DragPerformEvent>(_ =>
            {
                target.RemoveFromClassList("vrb-scene-ref__drop--hover");
                if (!HasDraggedGameObject() || reference == null)
                {
                    return;
                }

                DragAndDrop.AcceptDrag();

                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject go)
                    {
                        HandleDroppedGameObject(go, reference, changeCallback);
                    }
                }
            });
        }

        private static bool HasDraggedGameObject()
        {
            return DragAndDrop.objectReferences != null
                && DragAndDrop.objectReferences.Any(o => o is GameObject);
        }

        private void HandleDroppedGameObject(GameObject droppedObject, ProcessSceneReferenceBase reference, Action<object> changeCallback)
        {
            if (droppedObject == null) return;

            ProcessSceneObject pso = droppedObject.GetComponent<ProcessSceneObject>();
            if (pso == null)
            {
                pso = droppedObject.AddComponent<ProcessSceneObject>();
                EditorUtility.SetDirty(droppedObject);
            }

            if (pso == null || pso.Guid == Guid.Empty)
            {
                return;
            }

            List<Guid> oldGuids = reference.Guids.ToList();
            List<Guid> newGuids = new List<Guid> { pso.Guid };
            if (oldGuids.SequenceEqual(newGuids))
            {
                return;
            }

            ChangeValue(
                getNewValueCallback: () => { reference.ResetGuids(newGuids); return reference; },
                getOldValueCallback: () => { reference.ResetGuids(oldGuids); return reference; },
                assignValueCallback: changeCallback);
        }

        private void ClearReference(ProcessSceneReferenceBase reference, Action<object> changeCallback)
        {
            if (reference == null || reference.IsEmpty()) return;

            List<Guid> oldGuids = reference.Guids.ToList();
            ChangeValue(
                getNewValueCallback: () => { reference.ResetGuids(); return reference; },
                getOldValueCallback: () => { reference.ResetGuids(oldGuids); return reference; },
                assignValueCallback: changeCallback);
        }

        private static string DescribeReference(ProcessSceneReferenceBase reference)
        {
            if (reference == null || reference.IsEmpty())
            {
                return "Drop a game object here to assign it";
            }

            if (!RuntimeConfigurator.Exists)
            {
                return $"{reference.Guids.Count} reference(s)";
            }

            List<string> labels = new List<string>();
            foreach (Guid guid in reference.Guids)
            {
                foreach (ISceneObject obj in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                {
                    if (obj?.GameObject != null)
                    {
                        labels.Add(obj.GameObject.name);
                    }
                }
            }

            return labels.Count > 0
                ? "Selected " + string.Join(", ", labels)
                : "No objects in scene match this reference";
        }

        private static void AppendUnconfiguredWarnings(VisualElement root, ProcessSceneReferenceBase reference)
        {
            if (!RuntimeConfigurator.Exists)
            {
                return;
            }

            Type valueType = reference.GetReferenceType();
            HashSet<GameObject> missingComponentGOs = new HashSet<GameObject>();

            foreach (Guid guid in reference.Guids)
            {
                foreach (ISceneObject obj in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                {
                    GameObject go = obj?.GameObject;
                    if (go != null && go.GetComponent(valueType) == null)
                    {
                        missingComponentGOs.Add(go);
                    }
                }
            }

            if (missingComponentGOs.Count == 0)
            {
                return;
            }

            HelpBox box = new HelpBox(
                $"Referenced object is missing the required '{valueType.Name}' component.",
                HelpBoxMessageType.Warning);
            box.AddToClassList("vrb-scene-ref__warning");
            root.Add(box);
        }
    }
}
