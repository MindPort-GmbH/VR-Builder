using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UI.Views;
using VRBuilder.Editor.UI.Windows;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Drawer for <see cref="ProcessSceneReferenceBase"/>.
    /// </summary>
    [DefaultProcessDrawer(typeof(ProcessSceneReferenceBase))]
    public class ProcessSceneReferenceDrawer : AbstractDrawer
    {
        protected bool isUndoOperation;
        protected bool isExpanded;

        private static readonly EditorIcon deleteIcon = new EditorIcon("icon_delete");
        private static readonly EditorIcon editIcon = new EditorIcon("icon_edit");
        private static readonly EditorIcon showIcon = new EditorIcon("icon_arrow_right");
        private static int buttonWidth = 24;

        protected GUIStyle richTextLabelStyle;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            ProcessSceneReferenceBase reference = (ProcessSceneReferenceBase)currentValue;
            Type valueType = reference.GetReferenceType();
            List<Guid> oldGuids = reference.Guids.ToList();
            Rect guiLineRect = rect;

            InitializeRichTextLabelStyle();

            DrawLabel(ref rect, ref guiLineRect, label);

            DrawLimitationWarnings(reference.Guids, reference.AllowMultipleValues, ref rect, ref guiLineRect);

            DrawDragAndDropArea(ref rect, changeValueCallback, reference, oldGuids, ref guiLineRect, label);

            DrawMisconfigurationOnSelectedGameObjects(reference, valueType, ref rect, ref guiLineRect);

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
                message = "This only supports a single scene object at a time.";
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
                guiLineRect = AddNewRectLine(ref originalRect);
                EditorGUI.HelpBox(guiLineRect, message, messageType);
            }

            return;
        }

        private void DrawDragAndDropArea(ref Rect rect, Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, ref Rect guiLineRect, GUIContent label)
        {
            Rect dropRect = guiLineRect;
            dropRect.height += EditorDrawingHelper.SingleLineHeight * 2;
            Action<GameObject> droppedGameObject = (GameObject selectedSceneObject) => HandleDroppedGameObject(changeValueCallback, reference, oldGuids, selectedSceneObject, dropRect);
            DropAreaGUI(ref rect, ref guiLineRect, reference, droppedGameObject, changeValueCallback, label);
        }

        private void DrawMisconfigurationOnSelectedGameObjects(ProcessSceneReferenceBase reference, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {

            // Find all GameObjects that are missing the the component "valueType" needed
            IEnumerable<GameObject> gameObjectsWithMissingConfiguration = reference.Guids
                .SelectMany(guidToDisplay => RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay))
                .Select(sceneObject => sceneObject.GameObject)
                .Where(sceneObject => sceneObject == null || sceneObject.GetComponent(valueType) == null)
                .Distinct();


            // Add FixIt all if more than one game object exist
            if (gameObjectsWithMissingConfiguration.Count() > 1)
            {
                guiLineRect = AddNewRectLine(ref originalRect, EditorDrawingHelper.SingleLineHeight);
                AddFixItAllButton(gameObjectsWithMissingConfiguration, valueType, ref originalRect, ref guiLineRect);
            }

            // Add FixIt on each component
            foreach (GameObject selectedGameObject in gameObjectsWithMissingConfiguration)
            {
                AddFixItButton(selectedGameObject, valueType, ref originalRect, ref guiLineRect);
            }
        }

        /// <summary>
        /// Draws the label for a given GUID depending on its type and existence in <see cref="SceneObjectGroups.Instance"/>.
        /// </summary>
        /// <param name="guidToDisplay">The GUID to display the label for.</param>
        private void DrawLabel(Guid guidToDisplay)
        {
            string label;

            ISceneObjectRegistry registry = RuntimeConfigurator.Configuration.SceneObjectRegistry;
            if (registry.ContainsGuid(guidToDisplay))
            {
                SceneObjectGroups.SceneObjectGroup group;
                if (SceneObjectGroups.Instance.TryGetGroup(guidToDisplay, out group))
                {
                    label = group.Label;
                }
                else
                {
                    label = SceneObjectGroups.UniqueGuidNameItalic;
                }
            }
            else
            {
                label = $"{SceneObjectGroups.GuidNotRegisteredText} - {guidToDisplay}.";
            }

            GUILayout.Label($"Group: {label}", richTextLabelStyle);
        }

        private void HandleDroppedGameObject(Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, GameObject selectedSceneObject, Rect dropDownRect)
        {
            if (selectedSceneObject != null)
            {
                ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

                if (processSceneObject == null)
                {
                    Guid newGuid = OpenMissingProcessSceneObjectDialog(selectedSceneObject);

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

                    IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups = new List<SceneObjectGroups.SceneObjectGroup>() { new SceneObjectGroups.SceneObjectGroup(SceneObjectGroups.UniqueGuidName, processSceneObject.Guid) };
                    availableGroups = availableGroups.Concat(SceneObjectGroups.Instance.Groups.Where(group => !oldGuids.Contains(group.Guid)));
                    DrawSearchableGroupListPopup(dropDownRect, onItemSelected, availableGroups);
                }
            }
        }

        /// <summary>
        /// Renders a drop area GUI for assigning groups to the behavior or condition.
        /// </summary>
        /// <param name="originalRect">The rect of the whole behavior or condition.</param>
        /// <param name="guiLineRect">The rect of the last drawn line.</param>
        /// <param name="dropAction">The action to perform when a game object is dropped.</param>
        protected void DropAreaGUI(ref Rect originalRect, ref Rect guiLineRect, ProcessSceneReferenceBase reference, Action<GameObject> dropAction, Action<object> changeValueCallback, GUIContent label)
        {
            Event evt = Event.current;

            // Measure the content size and determine how many lines the content will occupy
            string referenceValue = GetReferenceValue(reference);
            string tooltip = GetTooltip(reference);
            string boxContent = string.IsNullOrEmpty(referenceValue) ? "Drop a game object here to assign it or any of its groups" : $"Selected {referenceValue}";
            GUIContent content = new GUIContent(boxContent, tooltip);
            GUIStyle style = GUI.skin.box;

            int lines = CalculateContentLines(content, originalRect, style, 3 * buttonWidth + 16); // Adding 16 pixels for padding between buttons
            float dropdownHeight = EditorDrawingHelper.ButtonHeight + (lines - 1) * EditorDrawingHelper.SingleLineHeight;
            guiLineRect = AddNewRectLine(ref originalRect, dropdownHeight);

            GUILayout.BeginArea(guiLineRect);
            GUILayout.BeginHorizontal();
            GUILayout.Box(content, GUILayout.Height(dropdownHeight), GUILayout.ExpandWidth(true));

            if (GUILayout.Button(editIcon.Texture, GUILayout.Height(EditorDrawingHelper.ButtonHeight), GUILayout.MaxWidth(buttonWidth)))
            {
                Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup selectedGroup) =>
                {
                    AddGroup(reference, reference.Guids, selectedGroup.Guid, changeValueCallback);
                };

                var availableGroups = SceneObjectGroups.Instance.Groups.Where(group => !reference.Guids.Contains(group.Guid));
                DrawSearchableGroupListPopup(guiLineRect, onItemSelected, availableGroups);
            }

            if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.ButtonHeight), GUILayout.MaxWidth(buttonWidth)))
            {
                reference.ResetGuids();
            }

            if (GUILayout.Button(showIcon.Texture, GUILayout.Height(EditorDrawingHelper.ButtonHeight), GUILayout.MaxWidth(buttonWidth)))
            {
                SceneReferencesEditorWindow referencesWindow = EditorWindow.GetWindow<SceneReferencesEditorWindow>();
                referencesWindow.titleContent = new GUIContent($"{label.text} - Objects in Scene");
                referencesWindow.SetReference(reference);
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
                            dropAction(dragged_object);
                        }
                    }
                    break;
            }



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


        protected Guid OpenMissingProcessSceneObjectDialog(GameObject selectedSceneObject)
        {
            Guid guid = Guid.Empty;

            if (selectedSceneObject != null)
            {
                if (EditorUtility.DisplayDialog("No Process Scene Object component", "This object does not have a Process Scene Object component.\n" +
                    "A Process Scene Object component is required for the object to work with the VR Builder process.\n" +
                    "Do you want to add one now?", "Yes", "No"))
                {
                    guid = selectedSceneObject.AddComponent<ProcessSceneObject>().Guid;
                    EditorUtility.SetDirty(selectedSceneObject);
                }
            }
            return guid;
        }

        // TODO Has duplicated code with AddFixItButton. Should be refactored if we keep FixItButton
        // TODO Undo does not work properly here and on AddFixItButton e.g.: a GrabCondition its only removing The GrabbableProperty but not TouchableProperty, IntractableProperty and Rigidbody
        protected void AddFixItAllButton(IEnumerable<GameObject> selectedSceneObject, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            string warning = $"Some Scene Objects are not configured as {valueType.Name}";
            const string button = "Fix all";
            EditorGUI.HelpBox(guiLineRect, warning, MessageType.Warning);
            guiLineRect = AddNewRectLine(ref originalRect);

            if (GUI.Button(guiLineRect, button))
            {
                foreach (GameObject sceneObject in selectedSceneObject)
                {
                    // Only relevant for Undoing a Process Property.
                    bool isAlreadySceneObject = sceneObject.GetComponent<ProcessSceneObject>() != null && typeof(ISceneObjectProperty).IsAssignableFrom(valueType);
                    Component[] alreadyAttachedProperties = sceneObject.GetComponents(typeof(Component));

                    RevertableChangesHandler.Do(
                        new ProcessCommand(
                            () => SceneObjectAutomaticSetup(sceneObject, valueType),
                            () => UndoSceneObjectAutomaticSetup(sceneObject, valueType, isAlreadySceneObject, alreadyAttachedProperties)));
                }
            }
            guiLineRect = AddNewRectLine(ref originalRect);
        }

        protected void AddFixItButton(GameObject selectedSceneObject, Type valueType, ref Rect originalRect, ref Rect guiLineRect)
        {
            guiLineRect = AddNewRectLine(ref originalRect);

            string warning = $"{selectedSceneObject.name} is not configured as {valueType.Name}";
            const string button = "Fix it";
            EditorGUI.HelpBox(guiLineRect, warning, MessageType.Warning);
            guiLineRect = AddNewRectLine(ref originalRect);

            if (GUI.Button(guiLineRect, button))
            {
                // Only relevant for Undoing a Process Property.
                bool isAlreadySceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() != null && typeof(ISceneObjectProperty).IsAssignableFrom(valueType);
                Component[] alreadyAttachedProperties = selectedSceneObject.GetComponents(typeof(Component));

                RevertableChangesHandler.Do(
                    new ProcessCommand(
                        () => SceneObjectAutomaticSetup(selectedSceneObject, valueType),
                        () => UndoSceneObjectAutomaticSetup(selectedSceneObject, valueType, isAlreadySceneObject, alreadyAttachedProperties)));
            }
        }

        // ToDo suggesting to move this in to a helper class
        protected Rect AddNewRectLine(ref Rect currentRect, float height = float.MinValue)
        {
            Rect newRectLine = currentRect;
            newRectLine.height = height == float.MinValue ? EditorDrawingHelper.SingleLineHeight : height;
            newRectLine.y += currentRect.height + EditorDrawingHelper.VerticalSpacing;

            currentRect.height += height == float.MinValue ? EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing : height + EditorDrawingHelper.VerticalSpacing;
            return newRectLine;
        }

        // ToDo suggesting to move this in to a helper class
        protected void SceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() ?? selectedSceneObject.AddComponent<ProcessSceneObject>();

            if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
            {
                sceneObject.AddProcessProperty(valueType);
            }

            isUndoOperation = true;
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

        private void DrawSearchableGroupListPopup(Rect rect, Action<SceneObjectGroups.SceneObjectGroup> onItemSelected, IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups = null)
        {
            VisualTreeAsset searchableList = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SearchableList);
            VisualTreeAsset groupListItem = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SearchableListItem);

            SearchableGroupListPopup content = new SearchableGroupListPopup(onItemSelected, searchableList, groupListItem);

            if (availableGroups == null)
            {
                availableGroups = SceneObjectGroups.Instance.Groups;
            }

            content.SetAvailableGroups(availableGroups);
            content.SetWindowSize(windowWith: rect.width);

            UnityEditor.PopupWindow.Show(rect, content);
        }

        private void UndoSceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType, bool hadProcessComponent, Component[] alreadyAttachedProperties)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

            if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
            {
                sceneObject.RemoveProcessProperty(valueType, true, alreadyAttachedProperties);
            }

            if (hadProcessComponent == false)
            {
                UnityEngine.Object.DestroyImmediate((ProcessSceneObject)sceneObject);
            }

            isUndoOperation = true;
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
                // Note: 
                richTextLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true
                };
            }
        }
    }
}
