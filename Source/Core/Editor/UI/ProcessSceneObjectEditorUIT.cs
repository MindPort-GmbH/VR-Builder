// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    [CustomEditor(typeof(ProcessSceneObjectUIT), true)]
    public class ProcessSceneObjectEditorUIT : UnityEditor.Editor
    {
        public VisualTreeAsset ManageTagsPanel;
        public VisualTreeAsset RemovableTag;

        private VisualElement root;

        private void OnEnable()
        {
            CheckVisualTreeAssets();
            AddDefaultTag();
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Create the root VisualElement for the inspector
            VisualElement root = new VisualElement();

            // Clone the ManageTagsPanel and add it to the root
            ManageTagsPanel.CloneTree(root);

            // Setup the functionality for managing tags
            SetupTagManagement(root);

            // Return the root element which contains the custom UI
            return root;
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

        private void CheckVisualTreeAssets()
        {
            if (ManageTagsPanel == null)
            {
                Debug.LogError("ManageTagsPanel not set in the Inspector.");
            }

            if (RemovableTag == null)
            {
                Debug.LogError("RemovableTag not set in the Inspector.");
            }
        }

        /// <summary>
        /// Adds a default tag to the ProcessSceneObject when it has no other tags.
        /// </summary>
        private void AddDefaultTag()
        {
            ProcessSceneObject processSceneObject = target as ProcessSceneObject;
            if (processSceneObject != null && processSceneObject.Tags.Count() == 0)
            {
                SceneObjectTags.Tag defaultTag = SceneObjectTags.Instance.CreateTag(processSceneObject.GameObject.name, Guid.NewGuid());
                if (defaultTag != null)
                {
                    processSceneObject.AddTag(defaultTag.Guid);
                    EditorUtility.SetDirty(processSceneObject);
                }
            }
        }

        private void SetupTagManagement(VisualElement root)
        {
            // Retrieve the necessary elements and containers from the cloned tree
            var newTagTextField = root.Q<TextField>("NewTagTextField");
            var addNewTagButton = root.Q<Button>("NewTagButton");
            var addTagDropdown = root.Q<DropdownField>("AddTagDropdown");
            var addTagButton = root.Q<Button>("AddTagButton");
            var tagListContainer = root.Q<VisualElement>("TagContainer");

            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();
            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);
            FilterAvailableTags(tagContainers, availableTags);


            SetupAddTag(newTagTextField, addNewTagButton, tagContainers);
            SetupAddExistingTag(addTagDropdown, addTagButton, tagContainers);
            AddExistingTags(root, tagListContainer, tagContainers);
        }

        private void AddExistingTags(VisualElement root, VisualElement tagListContainer, List<ITagContainer> tagContainers)
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

        private static void SetupAddExistingTag(DropdownField addTagDropdown, Button addTagButton, List<ITagContainer> tagContainers)
        {

            // Populate dropdown with available tags
            addTagDropdown.choices = availableTags.Select(tag => tag.Label).ToList();
            addTagDropdown.value = availableTags.FirstOrDefault()?.Label ?? "";

            // Add Tag button logic (from dropdown)
            addTagButton.clicked += () =>
            {
                // Add existing tag logic (similar to "Add Tag" button in OnInspectorGUI)
                // This should add the selected tag from dropdown to each selected tag container
                // Example: Add selected tag from dropdown to tagContainers
            };

            // Display tags for each tag container
            foreach (var tagContainer in tagContainers)
            {
                // For each tag in the tag container, create UI elements (like labels, buttons)
                // to display and manage the tags (add/remove functionality)
                // Example: Display and manage tags in tagListContainer
            }

            // Logic to update UI based on selected objects and their tags
            // Example: Update UI when different objects are selected or tags are changed
        }

        private static void SetupAddTag(TextField newTagTextField, Button addNewTagButton, List<ITagContainer> tagContainers)
        {
            // Add change event listener to the new tag text field
            newTagTextField.RegisterValueChangedCallback(evt =>
            {
                EvaluateNewTagName(evt.newValue, addNewTagButton);
            });

            // Add New Tag button logic
            addNewTagButton.clicked += () =>
            {
                Guid guid = Guid.NewGuid();
                string newTagName = newTagTextField.value;

                Undo.RecordObject(SceneObjectTags.Instance, "Created tag");
                SceneObjectTags.Instance.CreateTag(newTagName, guid);
                EditorUtility.SetDirty(SceneObjectTags.Instance);

                foreach (ITagContainer container in tagContainers)
                {
                    Undo.RecordObject((UnityEngine.Object)container, "Added tag");
                    container.AddTag(guid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)container);
                }

                GUI.FocusControl("");
                newTagTextField.value = "";
            };
        }

        private static void EvaluateNewTagName(string newTag, Button addNewTagButton)
        {
            bool canCreateTag = SceneObjectTags.Instance.CanCreateTag(newTag);
            addNewTagButton.SetEnabled(canCreateTag);
        }

        private void AddTagElement(VisualElement container, SceneObjectTags.Tag tag, bool multiEditDistinct = false)
        {
            VisualElement tagElement = RemovableTag.CloneTree();
            IEnumerable<ISceneObject> objectsWithTag = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag.Guid);

            tagElement.Q<Button>("Button").clicked += () => RemoveTagElement(container, tagElement, tag);
            tagElement.Q<Label>("Count").text = objectsWithTag.ToString();

            Label tagLabel = tagElement.Q<Label>("Tag");
            tagLabel.text = tag.Label;
            if (multiEditDistinct)
            {
                tagLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            }

            container.Add(tagElement);
        }

        private void RemoveTagElement(VisualElement container, VisualElement tagElement, SceneObjectTags.Tag tag)
        {
            // Remove the tag from the ProcessSceneObject component
            // Example:
            // ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            // processSceneObject.RemoveTag(tag); // Replace with actual remove method

            container.Remove(tagElement);
        }
    }
}
