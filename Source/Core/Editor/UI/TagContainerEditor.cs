using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UndoRedo;
using System.Collections.Generic;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// Inspector for the <see cref="ProcessTagContainer"/> component.
    /// </summary>
    [CustomEditor(typeof(ProcessTagContainer))]
    [CanEditMultipleObjects]
    public class TagContainerEditor : UnityEditor.Editor
    {
        int selectedTagIndex = 0;
        string newTag = "";
        private static EditorIcon deleteIcon;

        private void OnEnable()
        {
            if(deleteIcon == null)
            {
                deleteIcon = new EditorIcon("icon_delete");
            }
        }

        public override void OnInspectorGUI()
        {
            List<ProcessTagContainer> tagContainers = targets.Where(t => t is ProcessTagContainer).Cast<ProcessTagContainer>().ToList();

            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

            // Add and create new tag
            EditorGUILayout.BeginHorizontal();

            newTag = EditorGUILayout.TextField(newTag);

            EditorGUI.BeginDisabledGroup(SceneObjectTags.Instance.CanCreateTag(newTag) == false);

            if (GUILayout.Button("Add New", GUILayout.Width(128)))
            {
                Guid guid = Guid.NewGuid();
                RevertableChangesHandler.Do(new ProcessCommand(
                    () =>
                    {
                        SceneObjectTags.Instance.CreateTag(newTag, guid);
                        EditorUtility.SetDirty(SceneObjectTags.Instance);
                        tagContainers.ForEach(container => container.AddTag(guid));
                    },
                    () =>
                    {
                        tagContainers.ForEach(container => container.RemoveTag(guid));
                        EditorUtility.SetDirty(SceneObjectTags.Instance);
                        SceneObjectTags.Instance.RemoveTag(guid);
                    }
                    ));

                newTag = "";
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            foreach(SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
            {
                if(tagContainers.All(c => c.HasTag(tag.Guid)))
                {
                    availableTags.RemoveAll(t => t.Guid == tag.Guid);
                }
            }            

            if (selectedTagIndex >= availableTags.Count() && availableTags.Count() > 0)
            {
                selectedTagIndex = availableTags.Count() - 1;
            }

            // Add existing tag
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(availableTags.Count() == 0);
            selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, availableTags.Select(tag => tag.Label).ToArray());

            if (GUILayout.Button("Add Tag", GUILayout.Width(128)))
            {
                List<ProcessTagContainer> processedContainers = tagContainers.Where(container => container.HasTag(availableTags[selectedTagIndex].Guid) == false).ToList();

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

            // List tags
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
                    List<ProcessTagContainer> processedContainers = tagContainers.Where(container => container.HasTag(guid)).ToList();

                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => processedContainers.ForEach(container => container.RemoveTag(guid)),
                        () => processedContainers.ForEach(container => container.AddTag(guid))
                        ));
                    break;
                }

                string label = SceneObjectTags.Instance.GetLabel(guid);
                if(tagContainers.Any(container => container.HasTag(guid) == false))
                {
                    label = $"<i>{label}</i>";
                }

                EditorGUILayout.LabelField(label, BuilderEditorStyles.Label);

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
