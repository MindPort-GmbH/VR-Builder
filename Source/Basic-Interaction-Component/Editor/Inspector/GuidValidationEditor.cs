using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.BasicInteraction.Validation;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.BasicInteraction.Inspector
{
    [CustomEditor(typeof(GuidValidation))]
    [CanEditMultipleObjects]
    public class GuidValidationEditor : UnityEditor.Editor
    {
        int selectedTagIndex = 0;
        private static EditorIcon deleteIcon;

        private void OnEnable()
        {
            if (deleteIcon == null)
            {
                deleteIcon = new EditorIcon("icon_delete");
            }
        }

        public override void OnInspectorGUI()
        {
            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();

            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);

            EditorGUILayout.LabelField("Scene object tags:");

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
    }
}