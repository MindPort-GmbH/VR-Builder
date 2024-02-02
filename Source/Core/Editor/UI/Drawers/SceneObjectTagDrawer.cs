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
    /// Drawer for <see cref="SceneObjectTagBase"/>.
    /// </summary>
    [DefaultProcessDrawer(typeof(SceneObjectTagBase))]
    public class SceneObjectTagDrawer : AbstractDrawer
    {
        private const string noComponentSelected = "<none>";
        protected bool isUndoOperation;

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            SceneObjectTagBase sceneObjectTags = (SceneObjectTagBase)currentValue;
            Type valueType = sceneObjectTags.GetReferenceType();
            List<Guid> oldGuids = sceneObjectTags.Guids;
            Rect guiLineRect = rect;

            DrawLimitationWarnings(sceneObjectTags.Guids, sceneObjectTags.MaxValuesAllowed, ref rect, ref guiLineRect);

            DrawModifyTagSelectionButton(changeValueCallback, sceneObjectTags, oldGuids, guiLineRect);

            DrawDragAndDropArea(ref rect, changeValueCallback, sceneObjectTags, oldGuids, ref guiLineRect);

            DrawMisconfigurationOnSelectedGameObjects(sceneObjectTags, valueType, ref rect, ref guiLineRect);

            DrawSelectedTagsAndGameObjects(sceneObjectTags, ref rect, ref guiLineRect);
            return rect;
        }

        private void DrawLimitationWarnings(List<Guid> currentGuidTags, int sceneObjectsLimit, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            int taggedObjects = 0;
            foreach (Guid guid in currentGuidTags)
            {
                taggedObjects += RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guid).Count();
            }

            if (taggedObjects > sceneObjectsLimit)
            {
                string warning = $"This only supports {sceneObjectsLimit} scene objects at a time.";
                EditorGUI.HelpBox(guiLineRect, warning, MessageType.Warning);
                guiLineRect = AddNewRectLine(ref originalRect);
            }
            else if (taggedObjects == 0)
            {
                string error = $"No objects found in scene. This will result in a null reference.";
                EditorGUI.HelpBox(guiLineRect, error, MessageType.Error);
                guiLineRect = AddNewRectLine(ref originalRect);
            }

            return;
        }

        private void DrawModifyTagSelectionButton(Action<object> changeValueCallback, SceneObjectTagBase sceneObjectTags, List<Guid> oldGuids, Rect guiLineRect)
        {
            if (GUI.Button(guiLineRect, "Modify Tag Selection"))
            {
                Action<List<SceneObjectTags.Tag>> onItemsSelected = (List<SceneObjectTags.Tag> selectedTags) =>
                {
                    IEnumerable<Guid> newGuids = selectedTags.Select(tag => tag.Guid);
                    SetNewTags(sceneObjectTags, oldGuids, newGuids, changeValueCallback);
                };
                OpenSearchableTagListWindow(onItemsSelected, preSelectTags: sceneObjectTags.Guids, title: "Assign Tags");
            }
        }

        private void DrawDragAndDropArea(ref Rect rect, Action<object> changeValueCallback, SceneObjectTagBase sceneObjectTags, List<Guid> oldGuids, ref Rect guiLineRect)
        {
            Action<GameObject> droppedGameObject = (GameObject selectedSceneObject) =>
            {
                if (selectedSceneObject != null)
                {
                    ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

                    if (processSceneObject == null)
                    {
                        Guid newGuid = OpenMissingProcessSceneObjectDialog(selectedSceneObject);

                        if (newGuid != Guid.Empty)
                        {
                            SetNewTags(sceneObjectTags, oldGuids, new List<Guid> { newGuid }, changeValueCallback);
                        }
                    }
                    else if (processSceneObject.AllTags.Count() == 1)
                    {
                        SetNewTags(sceneObjectTags, oldGuids, processSceneObject.AllTags, changeValueCallback);
                    }
                    else
                    {
                        // if the PSO has multiple tags we let the user decide which ones he wants to take
                        Action<List<SceneObjectTags.Tag>> onItemsSelected = (List<SceneObjectTags.Tag> selectedTags) =>
                        {
                            IEnumerable<Guid> newGuids = selectedTags.Select(tag => tag.Guid);
                            SetNewTags(sceneObjectTags, oldGuids, newGuids, changeValueCallback);
                        };

                        OpenSearchableTagListWindow(onItemsSelected, availableTags: processSceneObject.AllTags, title: $"Assign Tags from {selectedSceneObject.name}");
                    }
                }
            };
            DropAreaGUI(ref rect, ref guiLineRect, droppedGameObject);
        }

        private void DrawMisconfigurationOnSelectedGameObjects(SceneObjectTagBase sceneObjectTags, Type valueType, ref Rect rect, ref Rect guiLineRect)
        {
            // Find all GameObjects that are missing the the component "valueType" needed
            IEnumerable<GameObject> gameObjectsWithMissingConfiguration = sceneObjectTags.Guids
                .SelectMany(guidToDisplay => RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guidToDisplay))
                .Select(sceneObject => sceneObject.GameObject)
                .Where(sceneObject => sceneObject == null || sceneObject.GetComponent(valueType) == null)
                .Distinct();


            // Add FixIt all if more than one game object exist
            if (gameObjectsWithMissingConfiguration.Count() > 1)
            {
                AddFixItAllButton(gameObjectsWithMissingConfiguration, valueType, ref rect, ref guiLineRect);
            }

            // Add FixIt on each component
            foreach (GameObject selectedGameObject in gameObjectsWithMissingConfiguration)
            {
                AddFixItButton(selectedGameObject, valueType, ref rect, ref guiLineRect);
            }
        }

        private void DrawSelectedTagsAndGameObjects(SceneObjectTagBase nameReference, ref Rect originalRect, ref Rect guiLineRect)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (nameReference.Guids == null)
            {
                return;
            }

            if (nameReference.Guids.Count() > 0)
            {
                guiLineRect = AddNewRectLine(ref originalRect);
                GUI.Label(guiLineRect, "Registered objects in scene:");
            }

            //TODO: create foldout like in NonUniqueSceneObjectRegistryEditorWindow
            foreach (Guid guidToDisplay in nameReference.Guids)
            {
                IEnumerable<ISceneObject> sceneObjects = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(guidToDisplay);

                //TODO: tag will be null here if the tag was deleted we need to add a check and show a error message further up
                string label = SceneObjectTags.Instance.GetLabel(guidToDisplay);

                if (string.IsNullOrEmpty(label))
                {
                    label = $"Unique Tag ({guidToDisplay})";
                }

                guiLineRect = AddNewRectLine(ref originalRect);

                GUILayout.BeginArea(guiLineRect);
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                GUILayout.Label($"Tag: {label}");
                if (GUILayout.Button("Select"))
                {
                    //TODO: Select the objects in the scene
                }
                if (GUILayout.Button("Remove"))
                {
                    nameReference.Guids.Remove(guidToDisplay);
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                foreach (ISceneObject sceneObject in sceneObjects)
                {
                    guiLineRect = AddNewRectLine(ref originalRect);

                    GUILayout.BeginArea(guiLineRect);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    if (GUILayout.Button("Show"))
                    {
                        EditorGUIUtility.PingObject(sceneObject.GameObject);
                    }

                    GUILayout.Label($"{sceneObject.GameObject.name}");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            }
        }

        protected Guid OpenMissingProcessSceneObjectDialog(GameObject selectedSceneObject)
        {
            Guid guid = Guid.Empty;

            if (selectedSceneObject != null)
            {
                //TODO: Implement don't ask me again
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

        // ToDo move this in to a helper class
        protected Rect AddNewRectLine(ref Rect currentRect, float height = float.MinValue)
        {
            Rect newRectLine = currentRect;
            newRectLine.height = height == float.MinValue ? EditorDrawingHelper.SingleLineHeight : height;
            newRectLine.y += currentRect.height + EditorDrawingHelper.VerticalSpacing;

            currentRect.height += height == float.MinValue ? EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing : height + EditorDrawingHelper.VerticalSpacing;
            return newRectLine;
        }

        protected void SceneObjectAutomaticSetup(GameObject selectedSceneObject, Type valueType)
        {
            ISceneObject sceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>() ?? selectedSceneObject.AddComponent<ProcessSceneObject>();

            if (typeof(ISceneObjectProperty).IsAssignableFrom(valueType))
            {
                sceneObject.AddProcessProperty(valueType);
            }

            isUndoOperation = true;
        }

        private void SetNewTags(SceneObjectTagBase nameReference, IEnumerable<Guid> oldGuids, IEnumerable<Guid> newGuids, Action<object> changeValueCallback)
        {
            bool containTheSameGuids = new HashSet<Guid>(oldGuids).SetEquals(newGuids);
            if (!containTheSameGuids)
            {
                ChangeValue(
                () =>
                {
                    nameReference.Guids = newGuids.ToList();
                    return nameReference;
                },
                () =>
                {
                    nameReference.Guids = oldGuids.ToList();
                    return nameReference;
                },
                changeValueCallback);
            }
        }

        /// <summary>
        /// Renders a drop area GUI for assigning tags to the behavior or condition.
        /// </summary>
        /// <param name="originalRect">The rect of the whole behavior or condition.</param>
        /// <param name="guiLineRect">The rect of the last drawn line.</param>
        /// <param name="dropAction">The action to perform when a game object is dropped.</param>
        private void DropAreaGUI(ref Rect originalRect, ref Rect guiLineRect, Action<GameObject> dropAction)
        {
            Event evt = Event.current;

            // TODO improve visuals style of drag and drop field
            guiLineRect = AddNewRectLine(ref originalRect, EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.SingleLineHeight);
            GUILayout.BeginArea(guiLineRect);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorDrawingHelper.IndentationWidth);
            GUILayout.Box($"To assign Tags Drop Game Object Here", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            //Texture2D texture = new Texture2D(1, 1);
            //GUILayout.Box(texture, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
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

        private void OpenSearchableTagListWindow(Action<List<SceneObjectTags.Tag>> selectedItemsCallback, IEnumerable<Guid> availableTags = null, IEnumerable<Guid> preSelectTags = null, string title = "Assign Tags")
        {
            var content = (SearchableTagListWindow)EditorWindow.GetWindow(typeof(SearchableTagListWindow), true, title);
            content.SetItemsSelectedCallBack(selectedItemsCallback);
            if (availableTags != null) content.UpdateAvailableTags(GetTags(availableTags));
            if (preSelectTags != null) content.PreSelectTags(GetTags(preSelectTags));
            //TODO: Set size and position if we do not change this window to a popup
        }

        private List<SceneObjectTags.Tag> GetTags(IEnumerable<Guid> tagsOnSceneObject)
        {
            List<SceneObjectTags.Tag> tags = new List<SceneObjectTags.Tag>();
            foreach (Guid guid in tagsOnSceneObject)
            {
                SceneObjectTags.Tag userDefinedTag = SceneObjectTags.Instance.Tags.FirstOrDefault(tag => guid == tag.Guid);
                if (userDefinedTag == default)
                {
                    tags.Add(new SceneObjectTags.Tag("[Default Tag]", guid, SceneObjectTags.TagType.Default));
                }
                else
                {
                    tags.Add(userDefinedTag);
                }
            }
            return tags;
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
    }
}
