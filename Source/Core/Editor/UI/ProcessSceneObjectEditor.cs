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
    public class ProcessSceneObjectEditor : UnityEditor.Editor
    {
        public VisualTreeAsset ManageTagsPanel;
        public VisualTreeAsset RemovableTag;
        public VisualTreeAsset NoTagsWarning;

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
            VisualElement root = new VisualElement();
            ManageTagsPanel.CloneTree(root);
            SetupTagManagement(root);
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

            if (NoTagsWarning == null)
            {
                Debug.LogError("NoTagsWarning not set in the Inspector.");
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
            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

            RemoveNonexistentTagsFromContainers(tagContainers);
            FilterAvailableTags(tagContainers, ref availableTags);

            SetupAddNewTagUI(newTagTextField, addNewTagButton, tagListContainer, tagContainers);
            SetupAddExistingTagUI(addTagDropdown, addTagButton, tagListContainer, tagContainers, availableTags);
            AddExistingTags(tagListContainer, tagContainers);
        }

        private void RemoveNonexistentTagsFromContainers(List<ITagContainer> tagContainers)
        {
            foreach (ITagContainer tagContainer in tagContainers)
            {
                List<Guid> tagsToRemove = new List<Guid>();

                foreach (Guid tagGuid in tagContainer.Tags)
                {
                    if (!SceneObjectTags.Instance.TagExists(tagGuid))
                    {
                        tagsToRemove.Add(tagGuid);
                    }
                }

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
            List<SceneObjectTags.Tag> usedTags = SceneObjectTags.Instance.Tags.Where(tag => tagContainers.Any(c => c.HasTag(tag.Guid))).ToList();

            foreach (SceneObjectTags.Tag tag in usedTags)
            {
                bool variesAcrossSelection = tagContainers.Any(container => container.HasTag(tag.Guid) == false);
                AddTagElement(tagListContainer, tag, variesAcrossSelection);
            }

            ValidateTagListContainer(tagListContainer);
        }

        private void ValidateTagListContainer(VisualElement tagListContainer)
        {
            bool containsTag = tagListContainer.Q<VisualElement>("RemovableTagContainer") != null;
            bool containsWarning = tagListContainer.Q<VisualElement>("NoTagsWarning") != null;

            if (!containsTag && !containsWarning)
            {
                VisualElement warning = NoTagsWarning.CloneTree();
                tagListContainer.Add(warning);
            }
            else if (containsTag && containsWarning)
            {
                tagListContainer.Q<Label>("NoTagsWarning").RemoveFromHierarchy();
            }
        }

        private void FilterAvailableTags(List<ITagContainer> tagContainers, ref List<SceneObjectTags.Tag> availableTags)
        {
            availableTags = SceneObjectTags.Instance.Tags.Where(tag => !tagContainers.All(c => c.HasTag(tag.Guid))).ToList();

            // Reset selectedTagIndex if it is out of bounds
            if (selectedTagIndex >= availableTags.Count && availableTags.Count > 0)
            {
                selectedTagIndex = availableTags.Count - 1;
            }
        }

        private void SetupAddExistingTagUI(DropdownField addTagDropdown, Button addTagButton, VisualElement tagListContainer, List<ITagContainer> tagContainers, List<SceneObjectTags.Tag> availableTags)
        {
            addTagDropdown.choices = availableTags.Select(tag => tag.Label).ToList();
            addTagDropdown.RegisterValueChangedCallback(evt => selectedTagIndex = addTagDropdown.choices.IndexOf(evt.newValue));

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

            addTagButton.clicked += () =>
            {
                if (selectedTagIndex >= 0 && selectedTagIndex < availableTags.Count)
                {
                    SceneObjectTags.Tag selectedTag = availableTags[selectedTagIndex];
                    AddTagToAll(tagContainers, selectedTag);
                    AddTagElement(tagListContainer, selectedTag);
                }
            };
        }

        private void SetupAddNewTagUI(TextField newTagTextField, Button addNewTagButton, VisualElement tagListContainer, List<ITagContainer> tagContainers)
        {
            EvaluateNewTagName(newTagTextField.text, addNewTagButton);
            newTagTextField.RegisterValueChangedCallback(evt => EvaluateNewTagName(evt.newValue, addNewTagButton));

            addNewTagButton.clicked += () =>
            {
                SceneObjectTags.Tag newTag = CreateTag(newTagTextField.text, tagContainers);
                AddTagElement(tagListContainer, newTag);
                newTagTextField.value = "";
            };
        }

        private SceneObjectTags.Tag CreateTag(string newTagName, List<ITagContainer> tagContainers)
        {
            Guid guid = Guid.NewGuid();
            Undo.RecordObject(SceneObjectTags.Instance, "Created tag");
            SceneObjectTags.Tag newTag = SceneObjectTags.Instance.CreateTag(newTagName, guid);
            AddTagToAll(tagContainers, newTag);
            return newTag;
        }

        private void AddTagToAll(List<ITagContainer> tagContainers, SceneObjectTags.Tag tag)
        {
            foreach (ITagContainer container in tagContainers)
            {
                Undo.RecordObject((UnityEngine.Object)container, "Added tag");
                container.AddTag(tag.Guid);
            }
        }

        private static void EvaluateNewTagName(string newTag, Button addNewTagButton)
        {
            addNewTagButton.SetEnabled(SceneObjectTags.Instance.CanCreateTag(newTag));
        }

        private void AddTagElement(VisualElement container, SceneObjectTags.Tag tag, bool variesAcrossSelection = false)
        {
            VisualElement tagElement = RemovableTag.CloneTree();
            Label tagLabel = tagElement.Q<Label>("Tag");
            tagLabel.text = tag.Label;
            if (variesAcrossSelection)
            {
                tagLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            }

            tagElement.Q<Label>("Count").text = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag.Guid).Count().ToString();
            tagElement.Q<Button>("Button").clicked += () => RemoveTagElement(container, tagElement, tag);
            container.Add(tagElement);
            ValidateTagListContainer(container);
        }

        private void RemoveTagElement(VisualElement container, VisualElement tagElement, SceneObjectTags.Tag tag)
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.RemoveTag(tag.Guid);
            container.Remove(tagElement);
            ValidateTagListContainer(container);
        }
    }
}
