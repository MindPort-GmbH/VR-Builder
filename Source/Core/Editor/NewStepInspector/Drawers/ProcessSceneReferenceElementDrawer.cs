// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Views;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit drawer for <see cref="ProcessSceneReferenceBase"/>.
    /// Provides a drop box for GameObjects, show/edit/delete buttons, and auto-setup
    /// of ProcessSceneObject components, matching the IMGUI ProcessSceneReferenceDrawer.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(ProcessSceneReferenceBase))]
    public class ProcessSceneReferenceElementDrawer : ElementDrawer
    {
        private static readonly EditorIcon deleteIcon = new EditorIcon("icon_delete");
        private static readonly EditorIcon editIcon = new EditorIcon("icon_edit");
        private static readonly EditorIcon showIcon = new EditorIcon("icon_info");

        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            ProcessSceneReferenceBase reference = (ProcessSceneReferenceBase)currentValue;

            VisualElement container = new VisualElement();
            container.style.marginBottom = 4;

            // Bold label
            if (!string.IsNullOrEmpty(label))
            {
                Label headerLabel = new Label(label);
                headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                headerLabel.style.marginBottom = 2;
                container.Add(headerLabel);
            }

            // Limitation warnings
            AddLimitationWarnings(container, reference);

            // Drop box row (horizontal: box + buttons)
            VisualElement dropRow = new VisualElement();
            dropRow.style.flexDirection = FlexDirection.Row;
            dropRow.style.alignItems = Align.Stretch;
            dropRow.style.minHeight = 24;
            dropRow.style.marginBottom = 2;

            // Box-style drop area matching IMGUI GUILayout.Box()
            string referenceValue = GetReferenceValue(reference);
            string boxContent = string.IsNullOrEmpty(referenceValue)
                ? "Drop a game object here to assign it or any of its groups"
                : $"Selected {referenceValue}";
            string tooltip = GetTooltip(reference);

            VisualElement dropBox = new VisualElement();
            dropBox.AddToClassList("scene-ref__drop-box");
            dropBox.tooltip = tooltip;

            // Show red error styling when no object is assigned
            bool isEmpty = reference.IsEmpty();
            if (isEmpty)
            {
                dropBox.AddToClassList("scene-ref__drop-box--empty");
            }

            Label dropLabel = new Label(boxContent);
            dropLabel.AddToClassList("scene-ref__drop-label");
            if (isEmpty)
            {
                dropLabel.AddToClassList("scene-ref__drop-label--empty");
            }
            dropBox.Add(dropLabel);

            // Handle drag-and-drop via UIToolkit events
            dropBox.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                {
                    bool hasGameObject = false;
                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject)
                        {
                            hasGameObject = true;
                            break;
                        }
                    }

                    if (hasGameObject)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        evt.StopPropagation();
                    }
                }
            });

            dropBox.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();

                if (DragAndDrop.objectReferences != null)
                {
                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject go)
                        {
                            HandleDroppedGameObject(changeValueCallback, reference, go);
                        }
                    }
                }

                evt.StopPropagation();
            });

            // Visual feedback on drag over
            dropBox.RegisterCallback<DragEnterEvent>(evt =>
            {
                dropBox.AddToClassList("scene-ref__drop-box--drag-over");
            });

            dropBox.RegisterCallback<DragLeaveEvent>(evt =>
            {
                dropBox.RemoveFromClassList("scene-ref__drop-box--drag-over");
            });

            dropRow.Add(dropBox);

            // Button column
            VisualElement buttonColumn = new VisualElement();
            buttonColumn.style.flexDirection = FlexDirection.Row;
            buttonColumn.style.alignItems = Align.Center;

            // Show button
            Button showButton = CreateSmallIconButton(showIcon, "Show referenced objects", () =>
            {
                OnShowReferencesClick(reference, changeValueCallback);
            });
            buttonColumn.Add(showButton);

            // Edit button
            Button editButton = CreateSmallIconButton(editIcon, "Edit references", () =>
            {
                OnEditReferencesClick(reference, changeValueCallback);
            });
            buttonColumn.Add(editButton);

            // Delete button
            Button clearButton = CreateSmallIconButton(deleteIcon, "Clear references", () =>
            {
                List<Guid> oldGuids = reference.Guids.ToList();
                ChangeValue(
                    () => { reference.ResetGuids(); return reference; },
                    () => { reference.ResetGuids(oldGuids); return reference; },
                    changeValueCallback);
            });
            buttonColumn.Add(clearButton);

            dropRow.Add(buttonColumn);
            container.Add(dropRow);

            // Handle unconfigured objects (Fix It buttons)
            AddUnconfiguredObjectWarnings(container, reference, changeValueCallback);

            return container;
        }

        private string GetReferenceValue(ProcessSceneReferenceBase reference)
        {
            if (reference.IsEmpty()) return "";
            return reference.ToString();
        }

        private string GetTooltip(ProcessSceneReferenceBase reference)
        {
            if (reference.IsEmpty()) return "No objects referenced";

            StringBuilder tooltip = new StringBuilder("Objects in scene:");
            try
            {
                foreach (Guid guid in reference.Guids)
                {
                    if (SceneObjectGroups.Instance.GroupExists(guid))
                    {
                        int count = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).Count();
                        tooltip.Append($"\n- Group '{SceneObjectGroups.Instance.GetLabel(guid)}': {count} objects");
                    }
                    else
                    {
                        foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                        {
                            tooltip.Append($"\n- {sceneObject.GameObject.name}");
                        }
                    }
                }
            }
            catch
            {
                // RuntimeConfigurator may not be available
            }

            return tooltip.ToString();
        }

        private void AddLimitationWarnings(VisualElement container, ProcessSceneReferenceBase reference)
        {
            if (!RuntimeConfigurator.Exists) return;

            try
            {
                int groupedObjectsCount = reference.Guids
                    .SelectMany(group => RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(group))
                    .Distinct().Count();

                string message = null;
                HelpBoxMessageType messageType = HelpBoxMessageType.Info;

                if (!reference.AllowMultipleValues && groupedObjectsCount > 1)
                {
                    message = "Multiple objects referenced. Only one will be used.";
                    messageType = HelpBoxMessageType.Warning;
                }
                else if (groupedObjectsCount == 0)
                {
                    if (SceneObjectGroups.Instance.Groups.Any(group => reference.Guids.Contains(group.Guid)))
                    {
                        message = "No objects found. A valid object must be spawned before this step.";
                        messageType = HelpBoxMessageType.Warning;
                    }
                    else if (reference.Guids.Any())
                    {
                        message = "No objects found in scene. This will result in a null reference.";
                        messageType = HelpBoxMessageType.Error;
                    }
                }

                if (!string.IsNullOrEmpty(message))
                {
                    HelpBox helpBox = new HelpBox(message, messageType);
                    helpBox.style.marginBottom = 4;
                    container.Add(helpBox);
                }
            }
            catch
            {
                // Runtime configuration may not be available
            }
        }

        private void HandleDroppedGameObject(Action<object> changeValueCallback, ProcessSceneReferenceBase reference, GameObject selectedSceneObject)
        {
            if (selectedSceneObject == null) return;

            List<Guid> oldGuids = reference.Guids.ToList();
            ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

            if (processSceneObject == null)
            {
                Guid newGuid = CreateProcessSceneObject(selectedSceneObject);
                if (newGuid != Guid.Empty)
                {
                    SetNewGroups(reference, oldGuids, new List<Guid> { newGuid }, changeValueCallback);
                }
            }
            else
            {
                IEnumerable<Guid> allGuids = GetAllGuids(processSceneObject);
                if (allGuids.Count() == 1)
                {
                    SetNewGroups(reference, oldGuids, allGuids, changeValueCallback);
                }
                else
                {
                    // Multiple groups - show selection popup
                    Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup selectedGroup) =>
                    {
                        SetNewGroup(reference, oldGuids, selectedGroup.Guid, changeValueCallback);
                    };

                    IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups =
                        new List<SceneObjectGroups.SceneObjectGroup> { new SceneObjectGroups.SceneObjectGroup(processSceneObject.gameObject.name, processSceneObject.Guid) };
                    availableGroups = availableGroups.Concat(
                        SceneObjectGroups.Instance.Groups.Where(group => processSceneObject.Guids.Contains(group.Guid)));

                    DrawSearchableGroupListPopup(onItemSelected, availableGroups, firstItemIsProcessSceneObject: true);
                }
            }
        }

        private void AddUnconfiguredObjectWarnings(VisualElement container, ProcessSceneReferenceBase reference, Action<object> changeValueCallback)
        {
            try
            {
                Type valueType = reference.GetReferenceType();

                IEnumerable<GameObject> unconfigured = reference.Guids
                    .SelectMany(guid => RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                    .Select(so => so.GameObject)
                    .Where(go => go == null || go.GetComponent(valueType) == null)
                    .Distinct();

                if (!unconfigured.Any()) return;

                if (AdvancedSettings.Instance.AutoAddProperties)
                {
                    // Auto-fix
                    Dictionary<GameObject, Component[]> alreadyAttached = new Dictionary<GameObject, Component[]>();
                    foreach (GameObject go in unconfigured)
                    {
                        alreadyAttached[go] = go.GetComponents(typeof(Component));
                    }

                    RevertableChangesHandler.Do(new ProcessCommand(
                        () =>
                        {
                            foreach (GameObject go in unconfigured)
                            {
                                SceneObjectExtensions.SceneObjectAutomaticSetup(go, valueType);
                            }
                        },
                        () =>
                        {
                            foreach (GameObject go in unconfigured)
                            {
                                SceneObjectExtensions.UndoSceneObjectAutomaticSetup(go, valueType, alreadyAttached[go]);
                            }
                        }));
                }
                else
                {
                    // Show Fix It buttons
                    if (unconfigured.Count() > 1)
                    {
                        AddFixItButton(container, unconfigured,
                            $"Some Scene Objects are not configured as {valueType.Name}", "Fix all", valueType);
                    }

                    foreach (GameObject go in unconfigured)
                    {
                        AddFixItButton(container, new[] { go },
                            $"{go.name} is not configured as {valueType.Name}", "Fix it", valueType);
                    }
                }
            }
            catch
            {
                // Runtime configuration may not be available
            }
        }

        private void AddFixItButton(VisualElement container, IEnumerable<GameObject> gameObjects, string warning, string buttonText, Type valueType)
        {
            HelpBox helpBox = new HelpBox(warning, HelpBoxMessageType.Warning);
            helpBox.style.marginBottom = 2;
            container.Add(helpBox);

            Button fixButton = new Button(() =>
            {
                foreach (GameObject go in gameObjects)
                {
                    Component[] alreadyAttached = go.GetComponents(typeof(Component));
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => SceneObjectExtensions.SceneObjectAutomaticSetup(go, valueType),
                        () => SceneObjectExtensions.UndoSceneObjectAutomaticSetup(go, valueType, alreadyAttached)));
                }
            });
            fixButton.text = buttonText;
            fixButton.style.marginBottom = 4;
            container.Add(fixButton);
        }

        private Guid CreateProcessSceneObject(GameObject selectedSceneObject)
        {
            Guid guid = Guid.Empty;
            if (selectedSceneObject == null) return guid;

            if (AdvancedSettings.Instance.AutoAddProcessSceneObject ||
                EditorUtility.DisplayDialog("No Process Scene Object component",
                    "This object does not have a Process Scene Object component.\n" +
                    "A Process Scene Object component is required for the object to work with the VR Builder process.\n" +
                    "Do you want to add one now?", "Yes", "No"))
            {
                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        guid = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                        EditorUtility.SetDirty(selectedSceneObject);
                    },
                    () => GameObject.DestroyImmediate(selectedSceneObject.GetComponent<ProcessSceneObject>())));
            }

            return guid;
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid> { obj.Guid }.Concat(obj.Guids);
        }

        private void SetNewGroups(ProcessSceneReferenceBase reference, IEnumerable<Guid> oldGuids, IEnumerable<Guid> newGuids, Action<object> changeValueCallback)
        {
            if (new HashSet<Guid>(oldGuids).SetEquals(newGuids)) return;

            ChangeValue(
                () => { reference.ResetGuids(newGuids); return reference; },
                () => { reference.ResetGuids(oldGuids); return reference; },
                changeValueCallback);
        }

        private void SetNewGroup(ProcessSceneReferenceBase reference, IEnumerable<Guid> oldGuids, Guid newGuid, Action<object> changeValueCallback)
        {
            if (oldGuids.Count() == 1 && oldGuids.Contains(newGuid)) return;

            ChangeValue(
                () => { reference.ResetGuids(new List<Guid> { newGuid }); return reference; },
                () => { reference.ResetGuids(oldGuids); return reference; },
                changeValueCallback);
        }

        private void OnShowReferencesClick(ProcessSceneReferenceBase reference, Action<object> changeValueCallback)
        {
            try
            {
                if (!reference.HasValue())
                {
                    if (reference.Guids.Any())
                    {
                        DrawSceneReferencesEditorPopup(reference, changeValueCallback);
                    }
                }
                else if (reference.Guids.Count() == 1 && !SceneObjectGroups.Instance.GroupExists(reference.Guids.First()))
                {
                    IEnumerable<ISceneObject> objects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(reference.Guids.First());
                    if (objects.Any())
                    {
                        EditorGUIUtility.PingObject(objects.First().GameObject);
                    }
                }
                else
                {
                    DrawSceneReferencesEditorPopup(reference, changeValueCallback);
                }
            }
            catch
            {
                // Runtime config may not be available
            }
        }

        private void OnEditReferencesClick(ProcessSceneReferenceBase reference, Action<object> changeValueCallback)
        {
            try
            {
                List<Guid> oldGuids = reference.Guids.ToList();
                Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup selectedGroup) =>
                {
                    // Add group to existing guids
                    if (!reference.Guids.Contains(selectedGroup.Guid))
                    {
                        ChangeValue(
                            () => { reference.ResetGuids(oldGuids.Concat(new[] { selectedGroup.Guid })); return reference; },
                            () => { reference.ResetGuids(oldGuids); return reference; },
                            changeValueCallback);
                    }
                };

                var availableGroups = SceneObjectGroups.Instance.Groups.Where(group => !reference.Guids.Contains(group.Guid));
                DrawSearchableGroupListPopup(onItemSelected, availableGroups);
            }
            catch
            {
                // SceneObjectGroups may not be available
            }
        }

        private void DrawSceneReferencesEditorPopup(ProcessSceneReferenceBase reference, Action<object> changeValueCallback)
        {
            SceneReferencesEditorPopup popup = new SceneReferencesEditorPopup(reference, changeValueCallback);
            popup.SetWindowSize(windowWith: 300);
            Rect popupRect = new Rect(GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : Vector2.zero), Vector2.zero);
            UnityEditor.PopupWindow.Show(popupRect, popup);
        }

        private void DrawSearchableGroupListPopup(Action<SceneObjectGroups.SceneObjectGroup> onItemSelected,
            IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups = null,
            bool firstItemIsProcessSceneObject = false)
        {
            VisualTreeAsset searchableList = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SearchableList);
            VisualTreeAsset groupListItem = ViewDictionary.LoadAsset(ViewDictionary.EnumType.GroupListItem);

            SearchableGroupListPopup content = new SearchableGroupListPopup(onItemSelected, searchableList, groupListItem);

            if (availableGroups == null)
            {
                availableGroups = SceneObjectGroups.Instance.Groups;
            }

            content.SetAvailableGroups(availableGroups, firstItemIsProcessSceneObject);
            content.SetWindowSize(windowWith: 300);

            Rect popupRect = new Rect(GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : Vector2.zero), Vector2.zero);
            UnityEditor.PopupWindow.Show(popupRect, content);
        }

        private static Button CreateSmallIconButton(EditorIcon icon, string tooltip, Action onClick)
        {
            Button button = new Button(() => onClick?.Invoke());
            button.tooltip = tooltip;
            button.AddToClassList("scene-ref__icon-button");

            try
            {
                Texture iconTexture = icon.Texture;
                if (iconTexture != null)
                {
                    Image iconImage = new Image { image = iconTexture };
                    iconImage.AddToClassList("scene-ref__icon-image");
                    button.Add(iconImage);
                }
            }
            catch
            {
                button.text = tooltip.Substring(0, 1);
            }

            return button;
        }
    }
}
