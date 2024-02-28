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
using VRBuilder.Editor.UndoRedo;

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

            //DrawObjectFields(tagContainers);
            DrawModifyTagSelectionButton(tagContainers);

            DrawDragAndDropArea(tagContainers);

            DrawSelectedTagsAndGameObjects(tagContainers.First());

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);

            //DrawTagsDropdown(tagContainers, availableTags);
        }

        private void DrawTagsDropdown(List<ITagContainer> tagContainers, List<SceneObjectTags.Tag> availableTags)
        {
            EditorGUILayout.LabelField("Allowed user tags");

            foreach (SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
            {
                if (tagContainers.All(c => c.HasTag(tag.Guid)))
                {
                    availableTags.RemoveAll(t => t.Guid == tag.Guid);
                }
            }

            if (selectedTagIndex >= availableTags.Count() && availableTags.Count() > 0)
            {
                selectedTagIndex = availableTags.Count() - 1;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(availableTags.Count() == 0);
            selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, availableTags.Select(tag => tag.Label).ToArray());

            if (GUILayout.Button("Add Tag", GUILayout.Width(128)))
            {
                List<ITagContainer> processedContainers = tagContainers.Where(container => container.HasTag(availableTags[selectedTagIndex].Guid) == false).ToList();

                RevertableChangesHandler.Do(new ProcessCommand(
                    () => processedContainers.ForEach(container => container.AddTag(availableTags[selectedTagIndex].Guid)),
                    () => processedContainers.ForEach(container => container.RemoveTag(availableTags[selectedTagIndex].Guid))
                    ));
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            List<SceneObjectTags.Tag> usedTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

            foreach (SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
            {
                if (tagContainers.All(c => c.HasTag(tag.Guid) == false))
                {
                    usedTags.RemoveAll(t => t.Guid == tag.Guid);
                }
            }

            foreach (Guid guid in usedTags.Select(t => t.Guid))
            {
                if (SceneObjectTags.Instance.TagExists(guid) == false)
                {
                    tagContainers.ForEach(c => c.RemoveTag(guid));
                    break;
                }

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.SingleLineHeight)))
                {
                    List<ITagContainer> processedContainers = tagContainers.Where(container => container.HasTag(guid)).ToList();

                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => processedContainers.ForEach(container => container.RemoveTag(guid)),
                        () => processedContainers.ForEach(container => container.AddTag(guid))
                        ));
                    break;
                }

                string label = SceneObjectTags.Instance.GetLabel(guid);
                if (tagContainers.Any(container => container.HasTag(guid) == false))
                {
                    label = $"<i>{label}</i>";
                }

                EditorGUILayout.LabelField(label, BuilderEditorStyles.Label);

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawObjectFields(IEnumerable<ITagContainer> tagContainers)
        {
            IEnumerable<IEnumerable<Guid>> selectedObjectGuids = tagContainers.Select(container => container.Tags.Where(guid => SceneObjectTags.Instance.Tags.Any(tag => tag.Guid == guid) == false));

            IEnumerable<Guid> commonGuids = selectedObjectGuids.Skip(1).Aggregate(selectedObjectGuids.First(), (current, next) => current.Intersect(next));

            foreach (Guid guid in commonGuids)
            {
                if (DrawObjectField(guid, tagContainers))
                {
                    break;
                }
            }

            DrawObjectField(Guid.Empty, tagContainers);
        }

        private bool DrawObjectField(Guid guid, IEnumerable<ITagContainer> tagContainers)
        {
            ProcessSceneObject oldProcessSceneObject = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid).FirstOrDefault() as ProcessSceneObject;
            ProcessSceneObject newProcessSceneObject = EditorGUILayout.ObjectField(oldProcessSceneObject, typeof(ProcessSceneObject), true) as ProcessSceneObject;

            if (newProcessSceneObject != oldProcessSceneObject)
            {
                if (newProcessSceneObject == null)
                {
                    foreach (ITagContainer tagContainer in tagContainers)
                    {
                        tagContainer.RemoveTag(oldProcessSceneObject.Guid);
                    }
                    return true;
                }
                else
                {
                    foreach (ITagContainer tagContainer in tagContainers)
                    {
                        if (tagContainer.HasTag(newProcessSceneObject.Guid) == false)
                        {
                            tagContainer.AddTag(newProcessSceneObject.Guid);
                        }
                    }
                }
            }

            return false;
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
            GUILayout.Box($"Drop a game object here to assign it or any of its tags", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
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

        private void DrawSelectedTagsAndGameObjects(ITagContainer tagContainer)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (tagContainer.Tags == null)
            {
                return;
            }

            if (tagContainer.Tags.Count() > 0)
            {
                GUILayout.Label("Registered objects in scene:");
            }

            //TODO Create foldout like in NonUniqueSceneObjectRegistryEditorWindow
            //TODO Need to improve the filtering and visuals of the list. E.g.: ProcessSceneObject count, Unique Tag, Not registered Tag.
            foreach (Guid guidToDisplay in tagContainer.Tags)
            {
                IEnumerable<ISceneObject> processSceneObjectsWithTag = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                DrawLabel(guidToDisplay);

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

        private void DrawLabel(Guid guidToDisplay)
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