using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
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
        private const string noComponentSelected = "<none>";
        protected bool isUndoOperation;
        protected string referenceValue;
        ProcessSceneReferenceBase reference;

        protected GUIStyle richTextLabelStyle;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            reference = (ProcessSceneReferenceBase)currentValue;
            Type valueType = reference.GetReferenceType();
            List<Guid> oldGuids = reference.Guids.ToList();
            Rect guiLineRect = rect;
            PopulateReferenceValueString(reference);

            InitializeRichTextLabelStyle();

            DrawLabel(ref rect, ref guiLineRect, label);

            DrawLimitationWarnings(reference.Guids, reference.AllowMultipleValues, ref rect, ref guiLineRect);

            DrawModifyGroupSelectionButton(changeValueCallback, reference, oldGuids, guiLineRect);

            DrawDragAndDropArea(ref rect, changeValueCallback, reference, oldGuids, ref guiLineRect);

            DrawMisconfigurationOnSelectedGameObjects(reference, valueType, ref rect, ref guiLineRect);

            if (reference.AllowMultipleValues)
            {
                DrawSelectedGroupsAndGameObjects(reference, ref rect, ref guiLineRect);
            }

            return rect;
        }

        private void PopulateReferenceValueString(ProcessSceneReferenceBase reference)
        {
            if (reference.HasValue() == false)
            {
                referenceValue = null;
            }
            else
            {
                referenceValue = reference.ToString();
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
            guiLineRect = AddNewRectLine(ref rect);
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
                message = "No objects found in scene. This will result in a null reference.";
                messageType = MessageType.Error;
            }

            if (!string.IsNullOrEmpty(message))
            {
                EditorGUI.HelpBox(guiLineRect, message, messageType);
                guiLineRect = AddNewRectLine(ref originalRect);
            }

            return;
        }

        private void DrawModifyGroupSelectionButton(Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, Rect guiLineRect)
        {
            if (GUI.Button(guiLineRect, "Modify Group Selection"))
            {
                Action<List<SceneObjectGroups.SceneObjectGroup>> onItemsSelected = (List<SceneObjectGroups.SceneObjectGroup> selectedGroups) =>
                {
                    IEnumerable<Guid> newGuids = selectedGroups.Select(group => group.Guid);
                    SetNewGroups(reference, oldGuids, newGuids, changeValueCallback);
                };
                OpenSearchableGroupListWindow(onItemsSelected, preSelectGroups: reference.Guids, title: "Assign Groups");
            }
        }

        private void DrawDragAndDropArea(ref Rect rect, Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, ref Rect guiLineRect)
        {
            Action<GameObject> droppedGameObject = (GameObject selectedSceneObject) => HandleDroppedGameObject(changeValueCallback, reference, oldGuids, selectedSceneObject);
            DropAreaGUI(ref rect, ref guiLineRect, droppedGameObject);
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

        private void DrawSelectedGroupsAndGameObjects(ProcessSceneReferenceBase reference, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (reference.Guids == null)
            {
                return;
            }

            if (reference.Guids.Count > 0)
            {
                guiLineRect = AddNewRectLine(ref originalRect);
                GUI.Label(guiLineRect, "Registered objects in scene:");
            }

            foreach (Guid guidToDisplay in reference.Guids)
            {
                IEnumerable<ISceneObject> processSceneObjectsWithGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);

                guiLineRect = AddNewRectLine(ref originalRect);

                GUILayout.BeginArea(guiLineRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                DrawLabel(guidToDisplay);
                if (GUILayout.Button("Select"))
                {
                    // Select all game objects with the group in the Hierarchy
                    Selection.objects = processSceneObjectsWithGroup.Select(processSceneObject => processSceneObject.GameObject).ToArray();
                }
                if (GUILayout.Button("Remove"))
                {
                    reference.RemoveGuid(guidToDisplay);
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                foreach (ISceneObject sceneObject in processSceneObjectsWithGroup)
                {
                    guiLineRect = AddNewRectLine(ref originalRect);

                    GUILayout.BeginArea(guiLineRect);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    GUILayout.Label($"{sceneObject.GameObject.name}");
                    if (GUILayout.Button("Show"))
                    {
                        EditorGUIUtility.PingObject(sceneObject.GameObject);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
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

        private void HandleDroppedGameObject(Action<object> changeValueCallback, ProcessSceneReferenceBase reference, List<Guid> oldGuids, GameObject selectedSceneObject)
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
                    Action<List<SceneObjectGroups.SceneObjectGroup>> onItemsSelected = (List<SceneObjectGroups.SceneObjectGroup> selectedGroups) =>
                    {
                        IEnumerable<Guid> newGuids = selectedGroups.Select(groups => groups.Guid);
                        SetNewGroups(reference, oldGuids, newGuids, changeValueCallback);
                    };

                    OpenSearchableGroupListWindow(onItemsSelected, availableGroups: GetAllGuids(processSceneObject), title: $"Assign Groups from {selectedSceneObject.name}");
                }
            }
        }

        /// <summary>
        /// Renders a drop area GUI for assigning groups to the behavior or condition.
        /// </summary>
        /// <param name="originalRect">The rect of the whole behavior or condition.</param>
        /// <param name="guiLineRect">The rect of the last drawn line.</param>
        /// <param name="dropAction">The action to perform when a game object is dropped.</param>
        protected void DropAreaGUI(ref Rect originalRect, ref Rect guiLineRect, Action<GameObject> dropAction)
        {
            Event evt = Event.current;

            // Measure the content size and determine how many lines the content will occupy
            GUIContent content = new GUIContent(string.IsNullOrEmpty(referenceValue) ? "Drop a game object here to assign it or any of its groups" : $"Selected: {referenceValue}");
            GUIStyle style = GUI.skin.box;
            int lines = CalculateContentLines(content, originalRect, style);

            guiLineRect = AddNewRectLine(ref originalRect, lines * EditorDrawingHelper.SingleLineHeight);
            GUILayout.BeginArea(guiLineRect);
            GUILayout.BeginHorizontal();
            GUILayout.Box(content, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (GUILayout.Button("X", GUILayout.MaxWidth(32)))
            {
                reference.ResetGuids();
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

        private int CalculateContentLines(GUIContent content, Rect originalRect, GUIStyle style)
        {
            Vector2 size = style.CalcSize(content);
            int lines = Mathf.CeilToInt(size.x / originalRect.width);
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

            guiLineRect = AddNewRectLine(ref originalRect);
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
            bool containTheSameGuids = new HashSet<Guid>(oldGuids).SetEquals(newGuids);
            if (!containTheSameGuids)
            {
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
        }

        private void OpenSearchableGroupListWindow(Action<List<SceneObjectGroups.SceneObjectGroup>> selectedItemsCallback, IEnumerable<Guid> availableGroups = null, IEnumerable<Guid> preSelectGroups = null, string title = "Assign Groups")
        {
            var content = (SearchableGroupListWindow)EditorWindow.GetWindow(typeof(SearchableGroupListWindow), true, title);
            content.SetItemsSelectedCallBack(selectedItemsCallback);
            if (availableGroups != null) content.UpdateAvailableGroups(GetGroups(availableGroups));
            if (preSelectGroups != null) content.PreSelectGroups(GetGroups(preSelectGroups));
        }

        private List<SceneObjectGroups.SceneObjectGroup> GetGroups(IEnumerable<Guid> groupsOnSceneObject)
        {
            List<SceneObjectGroups.SceneObjectGroup> groups = new List<SceneObjectGroups.SceneObjectGroup>();
            foreach (Guid guid in groupsOnSceneObject)
            {
                ISceneObjectRegistry registry = RuntimeConfigurator.Configuration.SceneObjectRegistry;
                if (registry.ContainsGuid(guid))
                {
                    SceneObjectGroups.SceneObjectGroup userDefinedGroup;
                    if (SceneObjectGroups.Instance.TryGetGroup(guid, out userDefinedGroup))
                    {
                        groups.Add(userDefinedGroup);
                    }
                    else
                    {
                        groups.Add(new SceneObjectGroups.SceneObjectGroup($"{SceneObjectGroups.UniqueGuidNameItalic}", guid));
                    }
                }
                else
                {
                    groups.Add(new SceneObjectGroups.SceneObjectGroup($"{SceneObjectGroups.GuidNotRegisteredText} - {guid}", guid));
                }
            }
            return groups;
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
