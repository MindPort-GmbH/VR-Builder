using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.BasicInteraction.Validation;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.BasicInteraction.Inspector
{
    [CustomEditor(typeof(HasGuidValidation))]
    [CanEditMultipleObjects]
    public class HasGuidValidationEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset searchableList;

        [SerializeField]
        private VisualTreeAsset tagListItem;
        protected GUIStyle richTextLabelStyle;

        private Rect lastButtonRect;

        public override void OnInspectorGUI()
        {
            InitializeRichTextLabelStyle();
            List<IGuidContainer> tagContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();
            List<SceneObjectTags.Tag> availableTags = SceneObjectTags.Instance.Tags.Where(tag => !tagContainers.All(c => c.HasGuid(tag.Guid))).ToList();
            Action<SceneObjectTags.Tag> onItemSelected = (SceneObjectTags.Tag selectedTag) => AddTag(selectedTag);

            EditorGUILayout.LabelField("<b>Allowed objects</b>", richTextLabelStyle);
            DrawDragAndDropArea(onItemSelected);
            DrawModifyTagSelectionButton(onItemSelected, availableTags);
            DrawSelectedTagsAndGameObjects(tagContainers);

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);
        }

        private void AddTag(SceneObjectTags.Tag selectedTag)
        {
            Guid guid = selectedTag.Guid;
            foreach (UnityEngine.Object target in targets)
            {
                IGuidContainer tagContainer = target as IGuidContainer;
                bool setDirty = false;

                if (tagContainer.Guids.Contains(guid) == false)
                {
                    tagContainer.AddGuid(guid);
                    setDirty = true;
                }

                if (setDirty)
                {
                    EditorUtility.SetDirty(target);
                }
            }
        }

        private void DrawDragAndDropArea(Action<SceneObjectTags.Tag> selectedItemCallback)
        {
            Action<GameObject> droppedGameObject = (GameObject selectedSceneObject) => HandleDroopedGameObject(selectedItemCallback, selectedSceneObject);
            DropAreaGUI(droppedGameObject);
        }

        private void HandleDroopedGameObject(Action<SceneObjectTags.Tag> selectedItemCallback, GameObject selectedSceneObject)
        {
            if (selectedSceneObject != null)
            {
                ProcessSceneObject processSceneObject = selectedSceneObject.GetComponent<ProcessSceneObject>();

                if (processSceneObject == null)
                {
                    Guid newGuid = OpenMissingProcessSceneObjectDialog(selectedSceneObject);

                    if (newGuid != Guid.Empty)
                    {
                        selectedItemCallback?.Invoke(GetTag(newGuid));
                    }
                }
                else
                {
                    var guids = GetAllGuids(processSceneObject);
                    if (guids.Count() == 1)
                    {
                        selectedItemCallback?.Invoke(GetTag(guids.First()));
                    }
                    else
                    {
                        // If the PSO has multiple tags we let the user decide which one he wants to take
                        OpenSearchableTagListDropdown(selectedItemCallback, GetTags(GetAllGuids(processSceneObject)));
                    }
                }
            }
        }

        private void DrawModifyTagSelectionButton(Action<SceneObjectTags.Tag> onItemSelected, List<SceneObjectTags.Tag> availableTags)
        {
            if (GUILayout.Button("Add tags"))
            {
                OpenSearchableTagListDropdown(onItemSelected, availableTags);
            }

            ///  Unity's GUILayout system doesn't allow for direct querying of element dimensions or positions before they are rendered. 
            ///  This is a workaround to get the position until we convert this component fully to UI Toolkit. 
            if (Event.current.type == EventType.Repaint)
            {
                lastButtonRect = GUILayoutUtility.GetLastRect();
            }
        }

        private void OpenSearchableTagListDropdown(Action<SceneObjectTags.Tag> selectedItemCallback, List<SceneObjectTags.Tag> availableTags = null)
        {
            SearchableTagListPopup content = new SearchableTagListPopup(selectedItemCallback, searchableList, tagListItem);
            content.SetAvailableTags(availableTags);
            content.SetWindowSize(windowWith: lastButtonRect.width);
            UnityEditor.PopupWindow.Show(lastButtonRect, content);
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

        private List<SceneObjectTags.Tag> GetTags(IEnumerable<Guid> tagsOnSceneObject)
        {
            List<SceneObjectTags.Tag> tags = new List<SceneObjectTags.Tag>();
            foreach (Guid guid in tagsOnSceneObject)
            {
                tags.Add(GetTag(guid));
            }
            return tags;
        }

        private SceneObjectTags.Tag GetTag(Guid guid)
        {
            SceneObjectTags.Tag tag;

            if (!SceneObjectTags.Instance.TryGetTag(guid, out tag))
            {
                tag = new SceneObjectTags.Tag($"{SceneObjectTags.AutoGeneratedTagName}", guid);
            }

            return tag;
        }

        private IEnumerable<Guid> GetAllGuids(ISceneObject obj)
        {
            return new List<Guid>() { obj.Guid }.Concat(obj.Guids);
        }

        protected void DropAreaGUI(Action<GameObject> dropAction)
        {
            Event evt = Event.current;

            GUILayout.BeginHorizontal();
            GUILayout.Box($"Drop a game object on this component to assign it or any of its tags", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndHorizontal();

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

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

        private void DrawSelectedTagsAndGameObjects(IEnumerable<IGuidContainer> tagContainers)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            if (tagContainers.Any(tagContainer => tagContainer.Guids.Count() > 0))
            {
                GUILayout.Label("Registered objects in scene:");
            }

            List<Guid> displayedGuids = new List<Guid>();

            foreach (IGuidContainer tagContainer in tagContainers)
            {
                foreach (Guid guidToDisplay in tagContainer.Guids)
                {
                    if (displayedGuids.Contains(guidToDisplay))
                    {
                        continue;
                    }

                    displayedGuids.Add(guidToDisplay);

                    IEnumerable<ISceneObject> processSceneObjectsWithTag = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    DrawLabel(guidToDisplay, tagContainers.All(tagContainer => tagContainer.HasGuid(guidToDisplay)) == false);

                    EditorGUI.BeginDisabledGroup(processSceneObjectsWithTag.Count() == 0);
                    if (GUILayout.Button("Select"))
                    {
                        // Select all game objects with the tag in the Hierarchy
                        Selection.objects = processSceneObjectsWithTag.Select(processSceneObject => processSceneObject.GameObject).ToArray();
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Remove"))
                    {
                        tagContainer.RemoveGuid(guidToDisplay);
                        GUILayout.EndHorizontal();
                        return;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    foreach (ISceneObject sceneObject in processSceneObjectsWithTag)
                    {
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
                richTextLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true
                };
            }
        }
    }
}