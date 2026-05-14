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
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.References
{
    /// <summary>
    /// UIToolkit drawer for any <see cref="ProcessSceneReferenceBase"/>. Mirrors the legacy
    /// IMGUI layout: bold label, then a row with the drop-target box on the left and three
    /// icon buttons (Info / Edit / Delete) on the right.
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
                heading.style.unityFontStyleAndWeight = FontStyle.Bold;
                root.Add(heading);
            }

            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-scene-ref__row");
            row.style.flexDirection = FlexDirection.Row;

            Label dropBox = new Label(DescribeReference(reference))
            {
                tooltip = BuildTooltip(reference)
            };
            dropBox.AddToClassList("vrb-scene-ref__drop");
            dropBox.style.flexGrow = 1f;
            row.Add(dropBox);

            Button infoButton = new Button(() => OnInfoClicked(reference))
            {
                text = Icons.Info,
                tooltip = "Show — ping the referenced object in the scene"
            };
            infoButton.AddToClassList("vrb-scene-ref__icon-button");
            infoButton.AddToClassList("vrb-scene-ref__info");
            row.Add(infoButton);

            Button editButton = new Button(() => OnEditClicked(reference, changeCallback))
            {
                text = Icons.Edit,
                tooltip = "Edit groups — add a Scene Object Group to this reference"
            };
            editButton.AddToClassList("vrb-scene-ref__icon-button");
            editButton.AddToClassList("vrb-scene-ref__edit");
            row.Add(editButton);

            Button deleteButton = new Button(() => ClearReference(reference, changeCallback))
            {
                text = Icons.Delete,
                tooltip = "Clear the reference"
            };
            deleteButton.AddToClassList("vrb-scene-ref__icon-button");
            deleteButton.AddToClassList("vrb-scene-ref__delete");
            row.Add(deleteButton);

            root.Add(row);

            RegisterDropHandlers(dropBox, reference, changeCallback);

            if (reference != null)
            {
                AppendUnconfiguredWarnings(root, reference);
            }

            return root;
        }

        // ───────── drop target ─────────

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

        // ───────── icon buttons ─────────

        private static void OnInfoClicked(ProcessSceneReferenceBase reference)
        {
            if (reference == null || reference.IsEmpty() || !RuntimeConfigurator.Exists)
            {
                return;
            }

            // If the reference resolves to exactly one scene object, ping it in the Hierarchy.
            // Otherwise log a brief summary so the user can see why it's ambiguous.
            List<GameObject> referenced = new List<GameObject>();
            foreach (Guid guid in reference.Guids)
            {
                foreach (ISceneObject obj in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                {
                    if (obj?.GameObject != null)
                    {
                        referenced.Add(obj.GameObject);
                    }
                }
            }

            if (referenced.Count == 1)
            {
                EditorGUIUtility.PingObject(referenced[0]);
                return;
            }

            if (referenced.Count == 0)
            {
                UnityEngine.Debug.Log("Reference does not resolve to any scene object.");
                return;
            }

            UnityEngine.Debug.Log("Reference resolves to multiple objects: "
                + string.Join(", ", referenced.Select(go => go.name)));
        }

        private void OnEditClicked(ProcessSceneReferenceBase reference, Action<object> changeCallback)
        {
            if (reference == null)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            IEnumerable<SceneObjectGroups.SceneObjectGroup> available = SceneObjectGroups.Instance.Groups
                .Where(group => reference.Guids.Contains(group.Guid) == false)
                .OrderBy(group => group.Label);

            int items = 0;
            foreach (SceneObjectGroups.SceneObjectGroup group in available)
            {
                Guid captured = group.Guid;
                List<Guid> oldGuids = reference.Guids.ToList();
                List<Guid> newGuids = oldGuids.Concat(new[] { captured }).ToList();

                menu.AddItem(new GUIContent(string.IsNullOrEmpty(group.Label) ? "(unnamed)" : group.Label),
                    false,
                    () => ChangeValue(
                        getNewValueCallback: () => { reference.ResetGuids(newGuids); return reference; },
                        getOldValueCallback: () => { reference.ResetGuids(oldGuids); return reference; },
                        assignValueCallback: changeCallback));
                items++;
            }

            if (items == 0)
            {
                menu.AddDisabledItem(new GUIContent("No more groups available"));
            }

            menu.ShowAsContext();
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

        // ───────── presentation helpers ─────────

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
                if (SceneObjectGroups.Instance.GroupExists(guid))
                {
                    labels.Add($"Group: {SceneObjectGroups.Instance.GetLabel(guid)}");
                    continue;
                }

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

        private static string BuildTooltip(ProcessSceneReferenceBase reference)
        {
            if (reference == null || reference.IsEmpty())
            {
                return "Drop a Process Scene Object (or any GameObject) here to assign it.";
            }

            return DescribeReference(reference);
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
