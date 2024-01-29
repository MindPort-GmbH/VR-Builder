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
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.UI
{
    [CustomEditor(typeof(ProcessSceneObject))]
    [CanEditMultipleObjects]
    public class ProcessSceneObjectEditor : UnityEditor.Editor
    {
        [SerializeField]
        private VisualTreeAsset manageTagsPanel;
        [SerializeField]
        private VisualTreeAsset removableTag;
        [SerializeField]
        private VisualTreeAsset noTagsWarning;
        [SerializeField]
        private VisualTreeAsset searchableList;
        [SerializeField]
        private VisualTreeAsset tagListItem;

        private void OnEnable()
        {
            EditorUtils.CheckVisualTreeAssets(nameof(ProcessSceneObjectEditor), new List<VisualTreeAsset>() { manageTagsPanel, removableTag, noTagsWarning, searchableList, tagListItem });
            //AddDefaultTag();
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            manageTagsPanel.CloneTree(root);
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
            Button addTagButton = root.Q<Button>("AddTagButton");
            VisualElement tagListContainer = root.Q<VisualElement>("TagList");
            Label objectIdLabel = root.Q<Label>("ObjectId");

            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();

            // TODO implement button make unique and not unique warning
            DisplayObjectGuid(objectIdLabel);
            RemoveNonexistentTagsFromContainers(tagContainers);
            SetupAddNewTagUI(newTagTextField, addNewTagButton, tagListContainer, tagContainers);
            SetupSearchableTagListPopup(addTagButton, tagListContainer, tagContainers);
            AddExistingTags(tagListContainer, tagContainers);
        }

        private void DisplayObjectGuid(Label objectIdLabel)
        {
            IEnumerable<ProcessSceneObject> processSceneObjects = targets.Where(t => t is ProcessSceneObject).Cast<ProcessSceneObject>();
            objectIdLabel.SetEnabled(processSceneObjects.Count() == 1);
            objectIdLabel.text = processSceneObjects.Count() == 1 ? $"Object Id: {processSceneObjects.FirstOrDefault()?.Guid.ToString()}" : "Object Id: [Multiple values selected]";
        }

        /// <summary>
        /// Clean up tags that might be deleted from the global SceneObjectTags registry
        /// </summary>
        /// <param name="tagContainers"></param>
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
                    tagContainer.RemoveTag(tagGuid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)tagContainer);
                }
            }
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

        private void SetupSearchableTagListPopup(Button addTagButton, VisualElement tagListContainer, List<ITagContainer> tagContainers)
        {
            Action<SceneObjectTags.Tag> onItemSelected = (SceneObjectTags.Tag selectedTag) =>
            {
                AddTag(tagListContainer, tagContainers, selectedTag);
            };

            SearchableTagListPopup content = new SearchableTagListPopup(onItemSelected, searchableList, tagListItem);

            addTagButton.clicked += () =>
            {
                content.SetAvailableTags(GetAvailableTags());
                content.SetWindowSize(windowWith: addTagButton.resolvedStyle.width);

                UnityEditor.PopupWindow.Show(addTagButton.worldBound, content);
            };
        }

        private void ValidateTagListContainer(VisualElement tagListContainer)
        {
            bool containsTag = tagListContainer.Q<VisualElement>("RemovableTagContainer") != null;
            bool containsWarning = tagListContainer.Q<VisualElement>("NoTagsWarning") != null;

            if (!containsTag && !containsWarning)
            {
                VisualElement warning = noTagsWarning.CloneTree();
                tagListContainer.Add(warning);
            }
            else if (containsTag && containsWarning)
            {
                tagListContainer.Q<Label>("NoTagsWarning").RemoveFromHierarchy();
            }
        }

        private List<SceneObjectTags.Tag> GetAvailableTags()
        {
            List<ITagContainer> tagContainers = targets.Where(t => t is ITagContainer).Cast<ITagContainer>().ToList();
            List<SceneObjectTags.Tag> availableTags = SceneObjectTags.Instance.Tags.Where(tag => !tagContainers.All(c => c.HasTag(tag.Guid))).ToList();
            return availableTags;
        }

        private SceneObjectTags.Tag CreateTag(string newTagName, List<ITagContainer> tagContainers)
        {
            Guid guid = Guid.NewGuid();
            Undo.RecordObject(SceneObjectTags.Instance, "Created tag");
            SceneObjectTags.Tag newTag = SceneObjectTags.Instance.CreateTag(newTagName, guid);
            AddTagToAll(tagContainers, newTag);
            return newTag;
        }

        private void AddTag(VisualElement tagListContainer, List<ITagContainer> tagContainers, SceneObjectTags.Tag selectedTag)
        {
            AddTagToAll(tagContainers, selectedTag);
            AddTagElement(tagListContainer, selectedTag);
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
            VisualElement tagElement = removableTag.CloneTree();
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
