using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Views;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for <see cref="ProcessSceneReferenceBase"/>.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessSceneReferenceBase))]
    public class ProcessSceneReferenceDrawer : AbstractDrawer
    {
        protected bool isExpanded;

        private static readonly EditorIcon deleteIcon = new EditorIcon("icon_delete");
        private static readonly EditorIcon editIcon = new EditorIcon("icon_edit");
        private static readonly EditorIcon showIcon = new EditorIcon("icon_info");
        private static int buttonWidth = 24;

        protected GUIStyle richTextLabelStyle;
        protected GUIStyle dropBoxStyle;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            ProcessSceneReferenceBase reference = (ProcessSceneReferenceBase)currentValue;
            Type valueType = reference.GetReferenceType();
            List<Guid> oldGuids = reference.Guids.ToList();
            Rect guiLineRect = rect;

            InitializeRichTextLabelStyle();
            InitializeDropBoxStyle();

            DrawLabel(ref rect, ref guiLineRect, label);

            DrawLimitationWarnings(reference.Guids, reference.AllowMultipleValues, ref rect, ref guiLineRect);

            DrawDragAndDropArea(ref rect, changeValueCallback, reference, oldGuids, ref guiLineRect, label);

            HandleUnconfiguredObjects(reference, valueType, ref rect, ref guiLineRect);

            return rect;
        }

        private string GetReferenceValue(ProcessSceneReferenceBase reference)
        {
            if (reference.IsEmpty())
            {
                return "";
            }
            else
            {
                return reference.ToString();
            }
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid>() { obj.Guid }.Concat(obj.Guids);
        }

        private void DrawLabel(ref Rect rect, ref Rect guiLineRect, GUIContent label)
        {
            GUIContent boldLabel = new GUIContent(label) { text = $"<b>{label.text}</b>" };
            EditorGUI.LabelField(rect, boldLabel, richTextLabelStyle);
        }

        private void DrawLimitationWarnings(IEnumerable<Guid> currentObjectGroups, bool allowMultipleValues, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (!RuntimeConfigurator.Exists)
            {
                return;
            }

            int groupedObjectsCount = currentObjectGroups.SelectMany(group => RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(group)).Distinct().Count();

            string message = string.Empty;
            MessageType messageType = MessageType.None;

            if (!allowMultipleValues && groupedObjectsCount > 1)
            {
                message = "Multiple objects referenced. Only one will be used.";
                messageType = MessageType.Warning;
            }
            else if (groupedObjectsCount == 0)
            {
                if (SceneObjectGroups.Instance.Groups.Any(group => currentObjectGroups.Contains(group.Guid)))
                {
                    message = "No objects found. A valid object must be spawned before this step.";
                    messageType = MessageType.Warning;
                }
                else
                {
                    message = "No objects found in scene. This will result in a null reference.";
                    messageType = MessageType.Error;
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                guiLineRect = EditorDrawingHelper.AddNewRectLine(ref originalRect);
                EditorGUI.HelpBox(guiLineRect, message, messageType);
            }

            return;
        }

        private void DrawDragAndDropArea(ref Rect rect, Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, ref Rect guiLineRect, GUIContent label)
        {
            Action<GameObject, Rect> droppedGameObject = (GameObject selectedSceneObject, Rect dropRect) => HandleDroppedGameObject(changeValueCallback, reference, oldGuids, selectedSceneObject, dropRect);
            DropAreaGUI(ref rect, ref guiLineRect, reference, droppedGameObject, changeValueCallback, label);
        }

        private void HandleUnconfiguredObjects(ProcessSceneReferenceBase reference, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            // Find all GameObjects that are missing the the component "valueType" needed
            IEnumerable<GameObject> gameObjectsWithMissingConfiguration = reference.Guids
                .SelectMany(guidToDisplay => RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay))
                .Select(sceneObject => sceneObject.GameObject)
                .Where(sceneObject => sceneObject == null || sceneObject.GetComponent(valueType) == null)
                .Distinct();

            // Automatically fix objects if configuration says so
            if (AdvancedSettings.Instance.AutoAddProperties && gameObjectsWithMissingConfiguration.Count() > 0)
            {
                Dictionary<GameObject, Component[]> alreadyAttachedProperties = new Dictionary<GameObject, Component[]>();
                foreach (GameObject gameObject in gameObjectsWithMissingConfiguration)
                {
                    alreadyAttachedProperties.Add(gameObject, gameObject.GetComponents(typeof(Component)));
                }

                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        foreach (GameObject gameObject in gameObjectsWithMissingConfiguration)
                        {
                            SceneObjectExtensions.SceneObjectAutomaticSetup(gameObject, valueType);
                        }
                    },
                    () =>
                    {
                        foreach (GameObject gameObject in gameObjectsWithMissingConfiguration)
                        {
                            SceneObjectExtensions.UndoSceneObjectAutomaticSetup(gameObject, valueType, alreadyAttachedProperties[gameObject]);
                        }
                    }
                   ));
            }
            else
            {
                // Add FixIt all if more than one game object exist
                if (gameObjectsWithMissingConfiguration.Count() > 1)
                {
                    guiLineRect = EditorDrawingHelper.AddNewRectLine(ref originalRect, EditorDrawingHelper.SingleLineHeight);
                    AddFixItAllButton(gameObjectsWithMissingConfiguration, valueType, ref originalRect, ref guiLineRect);
                }

                // Add FixIt on each component
                foreach (GameObject selectedGameObject in gameObjectsWithMissingConfiguration)
                {
                    guiLineRect = EditorDrawingHelper.AddNewRectLine(ref originalRect, EditorDrawingHelper.SingleLineHeight);
                    AddFixItButton(selectedGameObject, valueType, ref originalRect, ref guiLineRect);
                }
            }
        }

        private void HandleDroppedGameObject(Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, GameObject selectedSceneObject, Rect dropDownRect)
        {
            if (selectedSceneObject != null)
            {
                ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

                if (processSceneObject == null)
                {
                    Guid newGuid = CreateProcessSceneObject(selectedSceneObject);

                    if (newGuid != Guid.Empty)
                    {
                        SetNewGroups(reference, oldGuids, new List<Guid> { newGuid }, changeValueCallback);
                    }
                }
                else if (GetAllGuids(processSceneObject).Count() == 1)
                {
                    SetNewGroups(reference, oldGuids, GetAllGuids(processSceneObject), changeValueCallback);
                }
                else
                {
                    // if the PSO has multiple groups we let the user decide which ones he wants to take
                    Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup selectedGroup) =>
                    {
                        SetNewGroup(reference, oldGuids, selectedGroup.Guid, changeValueCallback);
                    };

                    // Set availableGroups first item to the processSceneObject.Guid and then add all groups of the PSO
                    IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups = new List<SceneObjectGroups.SceneObjectGroup>() { new SceneObjectGroups.SceneObjectGroup(processSceneObject.gameObject.name, processSceneObject.Guid) };
                    availableGroups = availableGroups.Concat(SceneObjectGroups.Instance.Groups.Where(group => processSceneObject.Guids.Contains(group.Guid) == true));
                    DrawSearchableGroupListPopup(dropDownRect, onItemSelected, availableGroups, firstItemIsProcessSceneObject: true);
                }
            }
        }

        /// <summary>
        /// Renders a drop area GUI for assigning groups to the behavior or condition.
        /// </summary>
        /// <param name="originalRect">The rect of the whole behavior or condition.</param>
        /// <param name="guiLineRect">The rect of the last drawn line.</param>
        /// <param name="dropAction">The action to perform when a game object is dropped.</param>
        protected void DropAreaGUI(ref Rect originalRect, ref Rect guiLineRect, ProcessSceneReferenceBase reference, Action<GameObject, Rect> dropAction, Action<object> changeValueCallback, GUIContent label)
        {
            Event evt = Event.current;

            // Measure the content size and determine how many lines the content will occupy
            string referenceValue = GetReferenceValue(reference);
            string tooltip = GetTooltip(reference);
            string boxContent = string.IsNullOrEmpty(referenceValue) ? "Drop a game object here to assign it or any of its groups" : $"Selected {referenceValue}";
            GUIContent content = new GUIContent(boxContent, tooltip);
            GUIStyle style = GUI.skin.box;

            int lines = CalculateContentLines(content, originalRect, style, (3 * buttonWidth) + 16); // Adding 16 pixels for padding between buttons
            float dropdownHeight = EditorDrawingHelper.ButtonHeight + ((lines - 1) * EditorDrawingHelper.SingleLineHeight);
            guiLineRect = EditorDrawingHelper.AddNewRectLine(ref originalRect, dropdownHeight);
            Rect flyoutRect = guiLineRect;

            GUILayout.BeginArea(guiLineRect);
            GUILayout.BeginHorizontal();
            GUILayout.Box(content, GUILayout.Height(dropdownHeight), GUILayout.ExpandWidth(true));

            if (GUILayout.Button(showIcon.Texture, GUILayout.Height(EditorDrawingHelper.ButtonHeight), GUILayout.MaxWidth(buttonWidth)))
            {
                OnShowReferencesClick(reference, changeValueCallback, dropdownHeight, flyoutRect);
            }
            if (GUILayout.Button(editIcon.Texture, GUILayout.Height(EditorDrawingHelper.ButtonHeight), GUILayout.MaxWidth(buttonWidth)))
            {
                OnEditReferencesClick(reference, changeValueCallback, dropdownHeight, flyoutRect);
            }

            if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.ButtonHeight), GUILayout.MaxWidth(buttonWidth)))
            {
                reference.ResetGuids();
                changeValueCallback(reference);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!guiLineRect.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (GameObject dragged_object in DragAndDrop.objectReferences)
                        {
                            dropAction(dragged_object, flyoutRect);
                        }
                    }
                    break;
            }
        }

        private Rect OnEditReferencesClick(ProcessSceneReferenceBase reference, Action<object> changeValueCallback, float dropdownHeight, Rect flyoutRect)
        {
            Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup selectedGroup) =>
            {
                AddGroup(reference, reference.Guids, selectedGroup.Guid, changeValueCallback);
            };

            flyoutRect = SetupLocalFlyoutRect(GUILayoutUtility.GetLastRect(), dropdownHeight, flyoutRect.width);
            var availableGroups = SceneObjectGroups.Instance.Groups.Where(group => !reference.Guids.Contains(group.Guid));
            DrawSearchableGroupListPopup(flyoutRect, onItemSelected, availableGroups);
            return flyoutRect;
        }

        private void OnShowReferencesClick(ProcessSceneReferenceBase reference, Action<object> changeValueCallback, float dropdownHeight, Rect flyoutRect)
        {
            if (!reference.HasValue())
            {
                if (reference.Guids.Count() > 0)
                {
                    // we have deleted groups and want to to show them in the popup
                    flyoutRect = SetupLocalFlyoutRect(GUILayoutUtility.GetLastRect(), dropdownHeight, flyoutRect.width);
                    DrawSceneReferencesEditorPopup(reference, changeValueCallback, flyoutRect);
                }
            }
            else if (reference.Guids.Count() == 1 && !SceneObjectGroups.Instance.GroupExists(reference.Guids.First()))
            {
                // we have only one guid and it is a PSO so we want to ping the object
                IEnumerable<ISceneObject> processSceneObjectsWithGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(reference.Guids.First());
                EditorGUIUtility.PingObject(processSceneObjectsWithGroup.First().GameObject);
            }
            else
            {
                flyoutRect = SetupLocalFlyoutRect(GUILayoutUtility.GetLastRect(), dropdownHeight, flyoutRect.width);
                DrawSceneReferencesEditorPopup(reference, changeValueCallback, flyoutRect);
            }
        }

        private Rect SetupLocalFlyoutRect(Rect lastRect, float dropdownHeight, float flyoutRectWidth)
        {
            Rect editGroupDropdownRect = lastRect;
            editGroupDropdownRect.width = flyoutRectWidth;
            editGroupDropdownRect.y += dropdownHeight;
            return editGroupDropdownRect;
        }

        private void DrawSceneReferencesEditorPopup(ProcessSceneReferenceBase reference, Action<object> changeValueCallback, Rect flyoutRect)
        {
            SceneReferencesEditorPopup sceneReferencesEditorPopup = new SceneReferencesEditorPopup(reference, changeValueCallback);
            sceneReferencesEditorPopup.SetWindowSize(windowWith: flyoutRect.width);

            UnityEditor.PopupWindow.Show(flyoutRect, sceneReferencesEditorPopup);
        }

        private string GetTooltip(ProcessSceneReferenceBase reference)
        {
            if (reference.IsEmpty())
            {
                return "No objects referenced";
            }

            StringBuilder tooltip = new StringBuilder("Objects in scene:");

            foreach (Guid guid in reference.Guids)
            {
                if (SceneObjectGroups.Instance.GroupExists(guid))
                {
                    string label = SceneObjectGroups.Instance.GetLabel(guid);
                    int objectsInScene = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).Count();
                    tooltip.Append($"\n- Group '{SceneObjectGroups.Instance.GetLabel(guid)}': {objectsInScene} objects");
                }
                else
                {
                    foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid))
                    {
                        tooltip.Append($"\n- {sceneObject.GameObject.name}");
                    }
                }
            }

            return tooltip.ToString();
        }

        private int CalculateContentLines(GUIContent content, Rect originalRect, GUIStyle style, int totalButtonsWidth)
        {
            Vector2 size = style.CalcSize(content);
            int lines = Mathf.CeilToInt(size.x / (originalRect.width - totalButtonsWidth));
            return lines;
        }


        protected Guid CreateProcessSceneObject(GameObject selectedSceneObject)
        {
            Guid guid = Guid.Empty;

            if (selectedSceneObject == null)
            {
                return guid;
            }

            if (AdvancedSettings.Instance.AutoAddProcessSceneObject || EditorUtility.DisplayDialog("No Process Scene Object component", "This object does not have a Process Scene Object component.\n" +
                "A Process Scene Object component is required for the object to work with the VR Builder process.\n" +
                "Do you want to add one now?", "Yes", "No"))
            {
                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        guid = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                        EditorUtility.SetDirty(selectedSceneObject);
                    },
                    () => GameObject.DestroyImmediate(selectedSceneObject.GetComponent<ProcessSceneObject>())
                    ));
            }

            return guid;
        }

        private void DrawFixItButton(IEnumerable<GameObject> gameObjects, string warning, string buttonText, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            EditorGUI.HelpBox(guiLineRect, warning, MessageType.Warning);
            guiLineRect = EditorDrawingHelper.AddNewRectLine(ref originalRect);

            if (GUI.Button(guiLineRect, buttonText))
            {
                foreach (GameObject sceneObject in gameObjects)
                {
                    Component[] alreadyAttachedProperties = sceneObject.GetComponents(typeof(Component));
                    RevertableChangesHandler.Do(
                        new ProcessCommand(
                            () => SceneObjectExtensions.SceneObjectAutomaticSetup(sceneObject, valueType),
                            () => SceneObjectExtensions.UndoSceneObjectAutomaticSetup(sceneObject, valueType, alreadyAttachedProperties)));
                }
            }
            guiLineRect = EditorDrawingHelper.AddNewRectLine(ref originalRect);
        }

        protected void AddFixItAllButton(IEnumerable<GameObject> selectedSceneObjects, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            string warning = $"Some Scene Objects are not configured as {valueType.Name}";
            const string buttonText = "Fix all";
            DrawFixItButton(selectedSceneObjects, warning, buttonText, valueType, ref originalRect, ref guiLineRect);
        }

        protected void AddFixItButton(GameObject selectedSceneObject, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            string warning = $"{selectedSceneObject.name} is not configured as {valueType.Name}";
            const string buttonText = "Fix it";
            DrawFixItButton(new List<GameObject> { selectedSceneObject }, warning, buttonText, valueType, ref originalRect, ref guiLineRect);
        }

        private void SetNewGroups(ProcessSceneReferenceBase reference, IEnumerable<Guid> oldGuids, IEnumerable<Guid> newGuids, Action<object> changeValueCallback)
        {
            if (new HashSet<Guid>(oldGuids).SetEquals(newGuids))
            {
                return;
            }
            ChangeValue(
            () =>
            {
                reference.ResetGuids(newGuids);
                return reference;
            },
            () =>
            {
                reference.ResetGuids(oldGuids);
                return reference;
            },
            changeValueCallback);
        }

        private void SetNewGroup(ProcessSceneReferenceBase reference, IEnumerable<Guid> oldGuids, Guid newGuid, Action<object> changeValueCallback)
        {
            if (oldGuids.Count() == 1 && oldGuids.Contains(newGuid))
            {
                return;
            }
            ChangeValue(
                () =>
                {
                    reference.ResetGuids(new List<Guid> { newGuid });
                    return reference;
                },
                () =>
                {
                    reference.ResetGuids(oldGuids);
                    return reference;
                },
                changeValueCallback);
        }

        private void AddGroup(ProcessSceneReferenceBase reference, IEnumerable<Guid> oldGuids, Guid newGuid, Action<object> changeValueCallback)
        {
            if (oldGuids.Contains(newGuid))
            {
                return;
            }

            ChangeValue(
                () =>
                {
                    reference.ResetGuids(oldGuids.Concat(new List<Guid> { newGuid }));
                    return reference;
                },
                () =>
                {
                    reference.ResetGuids(oldGuids);
                    return reference;
                },
                changeValueCallback);
        }

        private void DrawSearchableGroupListPopup(Rect rect, Action<SceneObjectGroups.SceneObjectGroup> onItemSelected, IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups = null, bool firstItemIsProcessSceneObject = false)
        {
            VisualTreeAsset searchableList = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SearchableList);
            VisualTreeAsset groupListItem = ViewDictionary.LoadAsset(ViewDictionary.EnumType.GroupListItem);

            SearchableGroupListPopup content = new SearchableGroupListPopup(onItemSelected, searchableList, groupListItem);

            if (availableGroups == null)
            {
                availableGroups = SceneObjectGroups.Instance.Groups;
            }

            content.SetAvailableGroups(availableGroups, firstItemIsProcessSceneObject);
            content.SetWindowSize(windowWith: rect.width);

            UnityEditor.PopupWindow.Show(rect, content);
        }

        /// <summary>
        /// Initializes the rich text label style.
        /// </summary>
        /// <remarks>
        /// GUIStyle can only be used within OnGUI() and not in a constructor.
        /// </remarks>
        private void InitializeRichTextLabelStyle()
        {
            if (richTextLabelStyle == null)
            {
                richTextLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true
                };
            }
        }

        /// <summary>
        /// Initializes the drop box style style.
        /// </summary>
        /// <remarks>
        /// GUIStyle can only be used within OnGUI() and not in a constructor.
        /// </remarks>
        private void InitializeDropBoxStyle()
        {
            if (dropBoxStyle == null)
            {
                dropBoxStyle = new GUIStyle(GUI.skin.box);
                dropBoxStyle.normal.textColor = Color.white;
            }
        }
    }
}
