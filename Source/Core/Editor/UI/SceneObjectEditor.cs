// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// This class adds names to newly added entities.
    /// </summary>
    [CustomEditor(typeof(ProcessSceneObject))]
    [CanEditMultipleObjects]
    internal class SceneObjectEditor : UnityEditor.Editor
    {
        int selectedTagIndex = 0;
        string newTag = "";
        private static EditorIcon deleteIcon;

        private void OnEnable()
        {
            ProcessSceneObject sceneObject = target as ProcessSceneObject;

            if (sceneObject.Tags.Count() == 0)
            {
                SceneObjectTags.Tag defaultTag = SceneObjectTags.Instance.CreateTag(sceneObject.GameObject.name, Guid.NewGuid());
                if (defaultTag != null)
                {
                    sceneObject.AddTag(defaultTag.Guid);
                    EditorUtility.SetDirty(sceneObject);
                }
            }

            if (deleteIcon == null)
            {
                deleteIcon = new EditorIcon("icon_delete");
            }
        }

        [MenuItem("CONTEXT/ProcessSceneObject/Remove Process Properties", false)]
        private static void RemoveProcessProperties()
        {
            Component[] processProperties = Selection.activeGameObject.GetComponents(typeof(ProcessSceneObjectProperty));
            ISceneObject sceneObject = Selection.activeGameObject.GetComponent(typeof(ISceneObject)) as ISceneObject;

            foreach (Component processProperty in processProperties)
            {
                sceneObject.RemoveProcessProperty(processProperty, true);
            }
        }

        [MenuItem("CONTEXT/ProcessSceneObject/Remove Process Properties", true)]
        private static bool ValidateRemoveProcessProperties()
        {
            return Selection.activeGameObject.GetComponents(typeof(ProcessSceneObjectProperty)) != null;
        }

        public override void OnInspectorGUI()
        {
            DrawTags();
        }

        private void DrawTags()
        {
            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();

            EditorGUILayout.LabelField("Scene object tags:");
            AddNewTag(tagContainers);
            AddExistingTags(tagContainers);
        }

        private void AddNewTag(List<ITagContainer> tagContainers)
        {
            EditorGUILayout.BeginHorizontal();
            newTag = EditorGUILayout.TextField(newTag);
            bool canCreateTag = SceneObjectTags.Instance.CanCreateTag(newTag);

            EditorGUI.BeginDisabledGroup(!canCreateTag);
            if (GUILayout.Button("Add New", GUILayout.Width(128)))
            {
                Guid guid = Guid.NewGuid();
                Undo.RecordObject(SceneObjectTags.Instance, "Created tag");
                SceneObjectTags.Instance.CreateTag(newTag, guid);
                EditorUtility.SetDirty(SceneObjectTags.Instance);

                foreach (ITagContainer container in tagContainers)
                {
                    Undo.RecordObject((UnityEngine.Object)container, "Added tag");
                    container.AddTag(guid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)container);
                }

                GUI.FocusControl("");
                newTag = "";
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void AddExistingTags(List<ITagContainer> tagContainers)
        {
            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);
            FilterAvailableTags(tagContainers, availableTags);
            DisplayAddTagUI(tagContainers, availableTags);
            ListAndHandleUsedTags(tagContainers);
        }

        private void FilterAvailableTags(List<ITagContainer> tagContainers, List<SceneObjectTags.Tag> availableTags)
        {
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
        }

        private void DisplayAddTagUI(List<ITagContainer> tagContainers, List<SceneObjectTags.Tag> availableTags)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(availableTags.Count() == 0);
            selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, availableTags.Select(tag => tag.Label).ToArray());

            if (GUILayout.Button("Add Tag", GUILayout.Width(128)))
            {
                Guid selectedGuid = availableTags[selectedTagIndex].Guid;
                List<ITagContainer> processedContainers = tagContainers.Where(container => container.HasTag(selectedGuid) == false).ToList();

                foreach (ITagContainer container in processedContainers)
                {
                    Undo.RecordObject((UnityEngine.Object)container, "Added tag");
                    container.AddTag(selectedGuid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)container);
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void ListAndHandleUsedTags(List<ITagContainer> tagContainers)
        {
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
                if (!SceneObjectTags.Instance.TagExists(guid))
                {
                    tagContainers.ForEach(c => c.RemoveTag(guid));
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.SingleLineHeight)))
                {
                    RemoveTagFromContainers(tagContainers, guid);
                    continue;
                }

                DisplayTagLabel(tagContainers, guid);

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RemoveTagFromContainers(List<ITagContainer> tagContainers, Guid guid)
        {
            List<ITagContainer> processedContainers = tagContainers.Where(container => container.HasTag(guid)).ToList();
            foreach (ITagContainer container in processedContainers)
            {
                Undo.RecordObject((UnityEngine.Object)container, "Removed tag");
                container.RemoveTag(guid);
                PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)container);
            }
        }

        private void DisplayTagLabel(List<ITagContainer> tagContainers, Guid guid)
        {
            string label = SceneObjectTags.Instance.GetLabel(guid);
            if (tagContainers.Any(container => container.HasTag(guid) == false))
            {
                label = $"<i>{label}</i>";
            }
            EditorGUILayout.LabelField(label, BuilderEditorStyles.Label);
        }

        private bool IsPropertyOverridden(string propertyName)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(target);
                foreach (PropertyModification mod in modifications)
                {
                    if (mod.propertyPath == propertyName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DrawOverrideIndicator(Rect position)
        {
            Color originalColor = GUI.color;
            GUI.color = new Color(0.26f, 0.59f, 0.98f, 1f); // Unity's blue
            GUI.DrawTexture(new Rect(position.x, position.y, 2, position.height), EditorGUIUtility.whiteTexture);
            GUI.color = originalColor;
        }

        private void DisplayPropertyWithOverrideIndicator(string displayValue, bool isPropertyOverridden)
        {
            GUIStyle labelStyle = isPropertyOverridden ? new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold } : EditorStyles.label;

            Rect position = EditorGUILayout.GetControlRect();
            if (isPropertyOverridden)
            {
                DrawOverrideIndicator(position);
                position.x += 6; // Shift the content to the right a bit more to accommodate for the blue line
            }

            EditorGUI.LabelField(position, displayValue, labelStyle);
        }
    }
}
