using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.BasicInteraction.Validation;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.BasicInteraction.Inspector
{
    [CustomEditor(typeof(GuidValidation))]
    [CanEditMultipleObjects]
    public class GuidValidationEditor : UnityEditor.Editor
    {
        int selectedTagIndex = 0;
        private static EditorIcon deleteIcon;
        protected GUIStyle richTextLabelStyle;


        private void OnEnable()
        {
            if (deleteIcon == null)
            {
                deleteIcon = new EditorIcon("icon_delete");
            }
        }

        public override void OnInspectorGUI()
        {
            InitializeRichTextLabelStyle();
            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();

            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);

            EditorGUILayout.LabelField("Allowed process scene objects");

            DrawModifyTagSelectionButton(tagContainers);

            DrawDragAndDropArea(tagContainers);

            DrawSelectedTagsAndGameObjects(tagContainers);

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);
        }

        private void DrawModifyTagSelectionButton(IEnumerable<ITagContainer> tagContainers)
        {
            IEnumerable<IEnumerable<Guid>> containerTags = tagContainers.Select(container => container.Tags);
            IEnumerable<Guid> preSelectedTags = containerTags.Skip(1).Aggregate(containerTags.First(), (current, next) => current.Intersect(next));



            if (GUILayout.Button("Add Tags"))
            {
                Action<List<SceneObjectTags.Tag>> onItemsSelected = (List<SceneObjectTags.Tag> selectedTags) =>
                {
                    IEnumerable<Guid> newGuids = selectedTags.Select(tag => tag.Guid);
                    AddNewTags(tagContainers, newGuids);
                };
                OpenSearchableTagListWindow(onItemsSelected, preSelectTags: preSelectedTags, title: "Assign Tags");
            }
        }

        private void AddNewTags(IEnumerable<ITagContainer> tagContainers, IEnumerable<Guid> newGuids)
        {
            foreach (ITagContainer tagContainer in tagContainers)
            {
                foreach (Guid guid in newGuids)
                {
                    if (tagContainer.Tags.Contains(guid) == false)
                    {
                        tagContainer.AddTag(guid);
                    }
                }
            }
        }

        private void OpenSearchableTagListWindow(Action<List<SceneObjectTags.Tag>> selectedItemsCallback, IEnumerable<Guid> availableTags = null, IEnumerable<Guid> preSelectTags = null, string title = "Assign Tags")
        {
            var content = (SearchableTagListWindow)EditorWindow.GetWindow(typeof(SearchableTagListWindow), true, title);
            content.SetItemsSelectedCallBack(selectedItemsCallback);
            if (availableTags != null) content.UpdateAvailableTags(GetTags(availableTags));
            if (preSelectTags != null) content.PreSelectTags(GetTags(preSelectTags));
            //TODO Set size and position if we do not change this window to a popup
        }

        private void DrawDragAndDropArea(IEnumerable<ITagContainer> tagContainers)
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
                            AddNewTags(tagContainers, new List<Guid> { newGuid });
                        }
                    }
                    else if (GetAllGuids(processSceneObject).Count() == 1)
                    {
                        AddNewTags(tagContainers, GetAllGuids(processSceneObject));
                    }
                    else
                    {
                        // if the PSO has multiple tags we let the user decide which ones he wants to take
                        Action<List<SceneObjectTags.Tag>> onItemsSelected = (List<SceneObjectTags.Tag> selectedTags) =>
                        {
                            IEnumerable<Guid> newGuids = selectedTags.Select(tag => tag.Guid);
                            AddNewTags(tagContainers, newGuids);
                        };

                        OpenSearchableTagListWindow(onItemsSelected, availableTags: GetAllGuids(processSceneObject), title: $"Assign Tags from {selectedSceneObject.name}");
                    }
                }
            };
            DropAreaGUI(droppedGameObject);
        }

        protected Guid OpenMissingProcessSceneObjectDialog(GameObject selectedSceneObject)
        {
            Guid guid = Guid.Empty;

            if (selectedSceneObject != null)
            {
                //TODO Implement don't ask me again
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

        private List<SceneObjectTags.Tag> GetTags(IEnumerable<Guid> tagsOnSceneObject)
        {
            List<SceneObjectTags.Tag> tags = new List<SceneObjectTags.Tag>();
            foreach (Guid guid in tagsOnSceneObject)
            {
                ISceneObjectRegistry registry = RuntimeConfigurator.Configuration.SceneObjectRegistry;
                if (registry.ContainsGuid(guid))
                {
                    SceneObjectTags.Tag userDefinedTag;
                    if (SceneObjectTags.Instance.TryGetTag(guid, out userDefinedTag))
                    {
                        tags.Add(userDefinedTag);
                    }
                    else
                    {
                        tags.Add(new SceneObjectTags.Tag($"{SceneObjectTags.AutoGeneratedTagName}", guid));
                    }
                }
                else
                {
                    tags.Add(new SceneObjectTags.Tag($"{SceneObjectTags.NotRegisterTagName} - {guid}", guid));
                }
            }
            return tags;
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid>() { obj.Guid }.Concat(obj.Tags);
        }

        protected void DropAreaGUI(Action<GameObject> dropAction)
        {
            Event evt = Event.current;

            // TODO Improve visuals style of drag and drop field
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorDrawingHelper.IndentationWidth);
            GUILayout.Box($"Drop a game object on this component to assign it or any of its tags", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndHorizontal();

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    //if (!guiLineRect.Contains(evt.mousePosition))
                    //    return;

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

        private void DrawSelectedTagsAndGameObjects(IEnumerable<ITagContainer> tagContainers)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (tagContainers.Any(tagContainer => tagContainer.Tags.Count() > 0))
            {
                GUILayout.Label("Registered objects in scene:");
            }

            List<Guid> displayedGuids = new List<Guid>();

            foreach (ITagContainer tagContainer in tagContainers)
            {
                //TODO Create foldout like in NonUniqueSceneObjectRegistryEditorWindow
                //TODO Need to improve the filtering and visuals of the list. E.g.: ProcessSceneObject count, Unique Tag, Not registered Tag.
                foreach (Guid guidToDisplay in tagContainer.Tags)
                {
                    if (displayedGuids.Contains(guidToDisplay))
                    {
                        continue;
                    }

                    displayedGuids.Add(guidToDisplay);

                    IEnumerable<ISceneObject> processSceneObjectsWithTag = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    DrawLabel(guidToDisplay, tagContainers.All(tagContainer => tagContainer.HasTag(guidToDisplay)) == false);

                    EditorGUI.BeginDisabledGroup(processSceneObjectsWithTag.Count() == 0);
                    if (GUILayout.Button("Select"))
                    {
                        // Select all game objects with the tag in the Hierarchy
                        Selection.objects = processSceneObjectsWithTag.Select(processSceneObject => processSceneObject.GameObject).ToArray();
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Remove"))
                    {
                        tagContainer.RemoveTag(guidToDisplay);
                        GUILayout.EndHorizontal();
                        return;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    foreach (ISceneObject sceneObject in processSceneObjectsWithTag)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                        if (GUILayout.Button("Show"))
                        {
                            EditorGUIUtility.PingObject(sceneObject.GameObject);
                        }

                        GUILayout.Label($"{sceneObject.GameObject.name}");
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawLabel(Guid guidToDisplay, bool italicize)
        {
            string label;

            SceneObjectTags.Tag tag;
            if (SceneObjectTags.Instance.TryGetTag(guidToDisplay, out tag))
            {
                label = tag.Label;
            }
            else if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsGuid(guidToDisplay))
            {
                label = SceneObjectTags.AutoGeneratedTagName;
            }
            else
            {
                //TODO Add a button to recreate the tag?
                label = $"{SceneObjectTags.NotRegisterTagName} - {guidToDisplay}.";
            }

            if (italicize)
            {
                label = $"<i>{label}</i>";
            }

            GUILayout.Label($"Tag: {label}", richTextLabelStyle);
        }

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