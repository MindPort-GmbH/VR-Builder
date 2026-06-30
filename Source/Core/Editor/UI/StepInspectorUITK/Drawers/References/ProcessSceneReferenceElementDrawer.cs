using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;
using VRBuilder.Core.Editor.UndoRedo;
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
            if (reference == null || reference.IsEmpty())
            {
                dropBox.AddToClassList("vrb-scene-ref__drop--empty");
            }
            dropBox.style.flexGrow = 1f;
            row.Add(dropBox);

            Button infoButton = null;
            infoButton = new Button(() => OnInfoClicked(infoButton, reference, changeCallback))
            {
                text = Icons.Info,
                tooltip = "Show referenced objects and groups"
            };
            infoButton.AddToClassList("vrb-scene-ref__icon-button");
            infoButton.AddToClassList("vrb-scene-ref__info");
            row.Add(infoButton);

            Button editButton = null;
            editButton = new Button(() => OnEditClicked(editButton, reference, changeCallback))
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

            VisualElement fixUI = Validation.MissingReferenceFixUI.BuildFor(
                reference,
                onFixed: () => { /* RevertableChangesHandler already fired the standard notify cascade */ });
            if (fixUI != null)
            {
                root.Add(fixUI);
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
                        HandleDroppedGameObject(go, target, reference, changeCallback);
                    }
                }
            });
        }

        private static bool HasDraggedGameObject()
        {
            return DragAndDrop.objectReferences != null
                && DragAndDrop.objectReferences.Any(o => o is GameObject);
        }

        private void HandleDroppedGameObject(
            GameObject droppedObject,
            VisualElement activator,
            ProcessSceneReferenceBase reference,
            Action<object> changeCallback)
        {
            if (droppedObject == null)
            {
                return;
            }

            ProcessSceneObject processSceneObject = droppedObject.GetComponent<ProcessSceneObject>();
            List<Guid> oldGuids = reference.Guids.ToList();

            if (processSceneObject == null)
            {
                Guid newGuid = CreateProcessSceneObject(droppedObject);
                if (newGuid != Guid.Empty)
                {
                    SetNewGroups(reference, oldGuids, new List<Guid> { newGuid }, changeCallback);
                }

                return;
            }

            IEnumerable<Guid> allGuids = GetAllGuids(processSceneObject);
            if (allGuids.Count() == 1)
            {
                SetNewGroups(reference, oldGuids, allGuids, changeCallback);
                return;
            }

            Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = selectedGroup =>
            {
                SetNewGroup(reference, oldGuids, selectedGroup.Guid, changeCallback);
            };

            IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups =
                new List<SceneObjectGroups.SceneObjectGroup>
                {
                    new SceneObjectGroups.SceneObjectGroup(processSceneObject.gameObject.name, processSceneObject.Guid)
                };
            availableGroups = availableGroups.Concat(
                SceneObjectGroups.Instance.Groups.Where(group => processSceneObject.Guids.Contains(group.Guid)));

            GroupPickerPopup.Show(activator, availableGroups, onItemSelected, firstItemIsProcessSceneObject: true);
        }

        private static IEnumerable<Guid> GetAllGuids(ISceneObject sceneObject)
        {
            return new List<Guid> { sceneObject.Guid }.Concat(sceneObject.Guids);
        }

        private static Guid CreateProcessSceneObject(GameObject selectedSceneObject)
        {
            if (selectedSceneObject == null)
            {
                return Guid.Empty;
            }

            if (AdvancedSettings.Instance.AutoAddProcessSceneObject
                || EditorUtility.DisplayDialog(
                    "No Process Scene Object component",
                    "This object does not have a Process Scene Object component.\n" +
                    "A Process Scene Object component is required for the object to work with the VR Builder process.\n" +
                    "Do you want to add one now?",
                    "Yes",
                    "No"))
            {
                Guid guid = Guid.Empty;
                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        guid = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                        EditorUtility.SetDirty(selectedSceneObject);
                    },
                    () => UnityEngine.Object.DestroyImmediate(selectedSceneObject.GetComponent<ProcessSceneObject>())));

                return guid;
            }

            return Guid.Empty;
        }

        private void SetNewGroups(
            ProcessSceneReferenceBase reference,
            IEnumerable<Guid> oldGuids,
            IEnumerable<Guid> newGuids,
            Action<object> changeCallback)
        {
            if (new HashSet<Guid>(oldGuids).SetEquals(newGuids))
            {
                return;
            }

            ChangeValue(
                getNewValueCallback: () =>
                {
                    reference.ResetGuids(newGuids);
                    return reference;
                },
                getOldValueCallback: () =>
                {
                    reference.ResetGuids(oldGuids);
                    return reference;
                },
                assignValueCallback: changeCallback);
        }

        private void SetNewGroup(
            ProcessSceneReferenceBase reference,
            IEnumerable<Guid> oldGuids,
            Guid newGuid,
            Action<object> changeCallback)
        {
            List<Guid> oldGuidList = oldGuids.ToList();
            if (oldGuidList.Count == 1 && oldGuidList.Contains(newGuid))
            {
                return;
            }

            ChangeValue(
                getNewValueCallback: () =>
                {
                    reference.ResetGuids(new List<Guid> { newGuid });
                    return reference;
                },
                getOldValueCallback: () =>
                {
                    reference.ResetGuids(oldGuidList);
                    return reference;
                },
                assignValueCallback: changeCallback);
        }

        // ───────── icon buttons ─────────

        private static void OnInfoClicked(
            VisualElement activator,
            ProcessSceneReferenceBase reference,
            Action<object> changeCallback)
        {
            if (reference == null || !RuntimeConfigurator.Exists)
            {
                return;
            }

            if (reference.IsEmpty())
            {
                return;
            }

            if (!reference.HasValue())
            {
                if (reference.Guids.Count > 0)
                {
                    SceneReferencesPopup.Show(activator, reference, changeCallback);
                }

                return;
            }

            if (reference.Guids.Count == 1 && !SceneObjectGroups.Instance.GroupExists(reference.Guids.First()))
            {
                IEnumerable<ISceneObject> processSceneObjectsWithGroup =
                    RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(reference.Guids.First());
                ISceneObject sceneObject = processSceneObjectsWithGroup.FirstOrDefault();
                if (sceneObject?.GameObject != null)
                {
                    EditorGUIUtility.PingObject(sceneObject.GameObject);
                }

                return;
            }

            SceneReferencesPopup.Show(activator, reference, changeCallback);
        }

        private void OnEditClicked(VisualElement activator, ProcessSceneReferenceBase reference, Action<object> changeCallback)
        {
            if (reference == null)
            {
                return;
            }

            IEnumerable<SceneObjectGroups.SceneObjectGroup> available = SceneObjectGroups.Instance.Groups
                .Where(group => reference.Guids.Contains(group.Guid) == false);

            GroupPickerPopup.Show(activator, available, group =>
            {
                List<Guid> oldGuids = reference.Guids.ToList();
                List<Guid> newGuids = oldGuids.Concat(new[] { group.Guid }).ToList();

                ChangeValue(
                    getNewValueCallback: () => { reference.ResetGuids(newGuids); return reference; },
                    getOldValueCallback: () => { reference.ResetGuids(oldGuids); return reference; },
                    assignValueCallback: changeCallback);
            });
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
                return "Drop a game object here to assign it or any of its groups";
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
                return "Drop a Process Scene Object (or any GameObject) here to assign it or any of its groups.";
            }

            if (!RuntimeConfigurator.Exists)
            {
                return DescribeReference(reference);
            }

            List<string> lines = new List<string> { "Objects in scene:" };

            foreach (Guid guid in reference.Guids)
            {
                if (SceneObjectGroups.Instance.GroupExists(guid))
                {
                    int objectsInScene = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).Count();
                    lines.Add($"- Group '{SceneObjectGroups.Instance.GetLabel(guid)}': {objectsInScene} objects");
                    continue;
                }

                foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                {
                    if (sceneObject?.GameObject != null)
                    {
                        lines.Add($"- {sceneObject.GameObject.name}");
                    }
                }
            }

            return string.Join("\n", lines);
        }
    }
}
