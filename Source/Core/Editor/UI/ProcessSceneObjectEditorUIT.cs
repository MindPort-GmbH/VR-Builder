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
    [CustomEditor(typeof(ProcessSceneObject))]
    [CanEditMultipleObjects]
    public class ProcessSceneObjectEditorUIT : UnityEditor.Editor
    {
        public VisualTreeAsset ManageTagsPanel;
        public VisualTreeAsset RemovableTag;

        private VisualElement root;

        /// <summary>
        /// Currently selected tag in the dropdown
        /// </summary>
        private int selectedTagIndex = 0;

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
            if (processSceneObject.Tags.Count() == 0)
            {
                SceneObjectTags.Tag defaultTag = SceneObjectTags.Instance.Tags.FirstOrDefault(tag => tag.Label == processSceneObject.GameObject.name);

                if (defaultTag == null)
                {
                    defaultTag = SceneObjectTags.Instance.CreateTag(processSceneObject.GameObject.name, Guid.NewGuid());
                }

                processSceneObject.AddTag(defaultTag.Guid);
                EditorUtility.SetDirty(processSceneObject);
            }
        }

        private void SetupTagManagement(VisualElement root)
        {
            // Retrieve the necessary elements and containers from the cloned tree
            TextField newTagTextField = root.Q<TextField>("NewTagTextField");
            Button addNewTagButton = root.Q<Button>("NewTagButton");
            DropdownField addTagDropdown = root.Q<DropdownField>("AddTagDropdown");
            Button addTagButton = root.Q<Button>("AddTagButton");
            VisualElement tagListContainer = root.Q<VisualElement>("TagList");

            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();
            RemoveNonexistentTagsFromContainers(tagContainers);

            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);
            FilterAvailableTags(tagContainers, ref availableTags);

            SetupAddNewTag(newTagTextField, addNewTagButton, tagListContainer, tagContainers);
            SetupAddExistingTag(addTagDropdown, addTagButton, tagListContainer, tagContainers, availableTags);
            AddExistingTags(tagListContainer, tagContainers);
        }

        private void RemoveNonexistentTagsFromContainers(List<ITagContainer> tagContainers)
        {
            foreach (ITagContainer tagContainer in tagContainers)
            {
                // Create a list to keep track of tags that need to be removed.
                List<Guid> tagsToRemove = new List<Guid>();

                // Check each tag in the container.
                foreach (Guid tagGuid in tagContainer.Tags)
                {
                    // If the tag does not exist in SceneObjectTags.Instance, mark it for removal.
                    if (!SceneObjectTags.Instance.TagExists(tagGuid))
                    {
                        tagsToRemove.Add(tagGuid);
                    }
                }

                // Remove the non-existent tags from the container.
                foreach (Guid tagGuid in tagsToRemove)
                {
                    Undo.RecordObject((UnityEngine.Object)tagContainer, "Removed non-existent tag");
                    tagContainer.RemoveTag(tagGuid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)tagContainer);
                }
            }
        }

        private void AddExistingTags(VisualElement tagListContainer, List<ITagContainer> tagContainers)
        {
            List<SceneObjectTags.Tag> usedTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

            foreach (SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
            {
                // If all tagContainers do not have this tag, remove it from the list of used tags.
                if (tagContainers.All(c => c.HasTag(tag.Guid) == false))
                {
                    usedTags.RemoveAll(t => t.Guid == tag.Guid);
                }
            }

            foreach (SceneObjectTags.Tag tag in usedTags.Select(t => t))
            {
                // Display the tag label. If any tagContainer doesn't have this tag, display it in italic.
                bool multiEditDistinc = false;
                if (tagContainers.Any(container => container.HasTag(tag.Guid) == false))
                {
                    multiEditDistinc = true;
                }
                AddTagElement(tagListContainer, tag, multiEditDistinc);
            }
            EditorUtility.SetDirty(target);
        }

        private void FilterAvailableTags(List<ITagContainer> tagContainers, ref List<SceneObjectTags.Tag> availableTags)
        {
            foreach (SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
            {
                if (tagContainers.All(c => c.HasTag(tag.Guid)))
                {
                    availableTags.RemoveAll(t => t.Guid == tag.Guid);
                }
            }

            // Reset selectedTagIndex if it is out of bounds
            if (selectedTagIndex >= availableTags.Count() && availableTags.Count() > 0)
            {
                selectedTagIndex = availableTags.Count() - 1;
            }
        }

        private void SetupAddExistingTag(DropdownField addTagDropdown, Button addTagButton, VisualElement tagListContainer, List<ITagContainer> tagContainers, List<SceneObjectTags.Tag> availableTags)
        {
            // Populate dropdown with available tags
            addTagDropdown.choices = availableTags.Select(tag => tag.Label).ToList();

            // Update selectedTagIndex based on the current selection
            addTagDropdown.RegisterValueChangedCallback(evt =>
            {
                selectedTagIndex = addTagDropdown.choices.IndexOf(evt.newValue);
            });

            // Set initial value
            if (availableTags.Any())
            {
                addTagDropdown.value = availableTags[0].Label;
                selectedTagIndex = 0;
            }
            else
            {
                addTagDropdown.value = "";
                selectedTagIndex = -1;
            }

            // Add Tag button logic
            addTagButton.clicked += () =>
            {
                if (selectedTagIndex >= 0 && selectedTagIndex < availableTags.Count)
                {
                    SceneObjectTags.Tag selectedTag = availableTags[selectedTagIndex];
                    AddTagToAll(tagContainers, selectedTag);
                    AddTagElement(tagListContainer, selectedTag);
                    EditorUtility.SetDirty(SceneObjectTags.Instance);
                }
            };
        }

        private void SetupAddNewTag(TextField newTagTextField, Button addNewTagButton, VisualElement tagListContainer, List<ITagContainer> tagContainers)
        {
            EvaluateNewTagName(newTagTextField.text, addNewTagButton);

            // Add change event listener to the new tag text field
            newTagTextField.RegisterValueChangedCallback(evt =>
            {
                EvaluateNewTagName(evt.newValue, addNewTagButton);
            });

            // Add New Tag button logic
            addNewTagButton.clicked += () =>
            {
                SceneObjectTags.Tag newTag = CreateTag(newTagTextField, tagListContainer, tagContainers);
                AddTagElement(tagListContainer, newTag);

                GUI.FocusControl("");
                newTagTextField.value = "";
            };
        }

        private SceneObjectTags.Tag CreateTag(TextField newTagTextField, VisualElement tagListContainer, List<ITagContainer> tagContainers)
        {
            Guid guid = Guid.NewGuid();
            string newTagName = newTagTextField.value;

            Undo.RecordObject(SceneObjectTags.Instance, "Created tag");
            SceneObjectTags.Tag newTag = SceneObjectTags.Instance.CreateTag(newTagName, guid);
            EditorUtility.SetDirty(SceneObjectTags.Instance);

            AddTagToAll(tagContainers, newTag);

            return newTag;
        }

        private void AddTagToAll(List<ITagContainer> tagContainers, SceneObjectTags.Tag tag)
        {
            foreach (ITagContainer container in tagContainers)
            {
                Undo.RecordObject((UnityEngine.Object)container, "Added tag");
                container.AddTag(tag.Guid);
                PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)container);
            }
        }

        private static void EvaluateNewTagName(string newTag, Button addNewTagButton)
        {
            bool canCreateTag = SceneObjectTags.Instance.CanCreateTag(newTag);
            addNewTagButton.SetEnabled(canCreateTag);
        }

        /// <summary>
        /// Add a visual tag element to the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="tag"></param>
        /// <param name="variesAcrossSelection"> This is used to indicate that the tag is not common across all selected objects.</param>
        private void AddTagElement(VisualElement container, SceneObjectTags.Tag tag, bool variesAcrossSelection = false)
        {
            VisualElement tagElement = RemovableTag.CloneTree();

            Label tagLabel = tagElement.Q<Label>("Tag");
            tagLabel.text = tag.Label;
            if (variesAcrossSelection)
            {
                tagLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            }

            Label countLabel = tagElement.Q<Label>("Count");
            IEnumerable<ISceneObject> objectsWithTag = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag.Guid);
            int count = objectsWithTag.Count();
            string countString = count > 1 ? count.ToString() : "unique";
            countLabel.text = $"({countString})";

            tagElement.Q<Button>("Button").clicked += () => RemoveTagElement(container, tagElement, tag);

            container.Add(tagElement);
        }



        private void RemoveTagElement(VisualElement container, VisualElement tagElement, SceneObjectTags.Tag tag)
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.RemoveTag(tag.Guid);

            container.Remove(tagElement);
        }
    }
}
