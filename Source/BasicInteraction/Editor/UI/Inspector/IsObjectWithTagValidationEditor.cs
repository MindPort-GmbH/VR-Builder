using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.BasicInteraction.Validation;
using VRBuilder.Core.Editor.UI;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.BasicInteraction.Editor.UI.Inspector
{
    [CustomEditor(typeof(IsObjectWithTagValidation))]
    [CanEditMultipleObjects]
    public class IsObjectWithTagValidationEditor : UnityEditor.Editor
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
            List<IGuidContainer> tagContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();
            var objectGroups = ServiceRegistry.Get<ISceneObjectRegistry>().SceneObjectGroups as SceneObjectGroups;

            List<SceneObjectGroups.SceneObjectGroup> availableTags = new List<SceneObjectGroups.SceneObjectGroup>(objectGroups.Groups);

            EditorGUILayout.Space(EditorDrawingHelper.VerticalSpacing);

            EditorGUILayout.LabelField("Scene object tags:");

            foreach (SceneObjectGroups.SceneObjectGroup tag in objectGroups.Groups)
            {
                if (tagContainers.All(c => c.HasGuid(tag.Guid)))
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
                List<IGuidContainer> processedContainers = tagContainers.Where(container => container.HasGuid(availableTags[selectedTagIndex].Guid) == false).ToList();

                RevertableChangesHandler.Do(new ProcessCommand(
                    () => processedContainers.ForEach(container => container.AddGuid(availableTags[selectedTagIndex].Guid)),
                    () => processedContainers.ForEach(container => container.RemoveGuid(availableTags[selectedTagIndex].Guid))
                    ));
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            List<SceneObjectGroups.SceneObjectGroup> usedTags = new List<SceneObjectGroups.SceneObjectGroup>(objectGroups.Groups);

            foreach (SceneObjectGroups.SceneObjectGroup tag in objectGroups.Groups)
            {
                if (tagContainers.All(c => c.HasGuid(tag.Guid) == false))
                {
                    usedTags.RemoveAll(t => t.Guid == tag.Guid);
                }
            }

            foreach (Guid guid in usedTags.Select(t => t.Guid))
            {
                if (ServiceRegistry.Get<ISceneObjectRegistry>().SceneObjectGroups.GroupExists(guid) == false)
                {
                    tagContainers.ForEach(c => c.RemoveGuid(guid));
                    break;
                }

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.SingleLineHeight)))
                {
                    List<IGuidContainer> processedContainers = tagContainers.Where(container => container.HasGuid(guid)).ToList();

                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => processedContainers.ForEach(container => container.RemoveGuid(guid)),
                        () => processedContainers.ForEach(container => container.AddGuid(guid))
                        ));
                    break;
                }

                string label = ServiceRegistry.Get<ISceneObjectRegistry>().SceneObjectGroups.GetLabel(guid);
                if (tagContainers.Any(container => container.HasGuid(guid) == false))
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