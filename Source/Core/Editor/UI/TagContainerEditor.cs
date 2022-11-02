using System.Linq;
using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UndoRedo;

namespace VRBuilder.Editor.UI
{
    [CustomEditor(typeof(ProcessTagContainer))]
    public class TagContainerEditor : UnityEditor.Editor
    {
        int selectedTagIndex = 0;

        public override void OnInspectorGUI()
        {
            ProcessTagContainer tagContainer = target as ProcessTagContainer;

            SceneObjectTags.Tag[] availableTags = SceneObjectTags.Instance.Tags.Where(tag => tagContainer.HasTag(tag.Guid) == false).ToArray();

            if (selectedTagIndex >= availableTags.Length && availableTags.Length > 0)
            {
                selectedTagIndex = availableTags.Length - 1;
            }

            EditorGUI.BeginDisabledGroup(availableTags.Length == 0);
            selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, availableTags.Select(tag => tag.Label).ToArray());

            if (GUILayout.Button("Add tag"))
            {
                RevertableChangesHandler.Do(new ProcessCommand(
                    () => tagContainer.AddTag(availableTags[selectedTagIndex].Guid),
                    () => tagContainer.RemoveTag(availableTags[selectedTagIndex].Guid)
                    ));

                tagContainer.AddTag(availableTags[selectedTagIndex].Guid);
            }
            EditorGUI.EndDisabledGroup();

            foreach (Guid tag in tagContainer.Tags)
            {
                if (SceneObjectTags.Instance.TagExists(tag) == false)
                {
                    tagContainer.RemoveTag(tag);
                    break;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(SceneObjectTags.Instance.GetLabel(tag));

                if (GUILayout.Button("X"))
                {
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => tagContainer.RemoveTag(tag),
                        () => tagContainer.AddTag(tag)
                        ));
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

    }
}
