// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

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
using VRBuilder.Unity;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// Custom editor for <see cref="ProcessSceneObject"/>.
    /// </summary>
    [CustomEditor(typeof(ProcessSceneObject))]
    [CanEditMultipleObjects]
    public class ProcessSceneObjectEditor : UnityEditor.Editor
    {
        public const string UniqueIdDisplayName = "Object Id";
        public const string AssetOnDiskText = "[Asset on disk]";
        public const string MultipleValuesSelectedText = "[Multiple values selected]";
        public const string TagCountNotAvailableText = "N/A";

        [SerializeField]
        private VisualTreeAsset manageTagsPanel;
        [SerializeField]
        private VisualTreeAsset removableTag;
        [SerializeField]
        private VisualTreeAsset noCustomTagsMessage;
        [SerializeField]
        private VisualTreeAsset searchableList;
        [SerializeField]
        private VisualTreeAsset tagListItem;

        private TextField newTagTextField;
        private Button addNewTagButton;
        private Button addTagButton;
        private VisualElement tagListContainer;
        private Label objectIdLabel;

        private void OnEnable()
        {
            EditorUtils.CheckVisualTreeAssets(nameof(ProcessSceneObjectEditor), new List<VisualTreeAsset>() { manageTagsPanel, removableTag, noCustomTagsMessage, searchableList, tagListItem });
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.UniqueIdChanged += OnUniqueIdChanged;
        }

        private void OnDisable()
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.UniqueIdChanged -= OnUniqueIdChanged;
        }

        private void OnUniqueIdChanged(object sender, UniqueIdChangedEventArgs e)
        {
            DisplayObjectGuid(objectIdLabel);
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

        private void SetupTagManagement(VisualElement root)
        {
            // Retrieve the necessary elements and containers from the cloned tree
            newTagTextField = root.Q<TextField>("NewTagTextField");
            addNewTagButton = root.Q<Button>("NewTagButton");
            addTagButton = root.Q<Button>("AddTagButton");
            tagListContainer = root.Q<VisualElement>("TagList");
            objectIdLabel = root.Q<Label>("ObjectId");

            List<IGuidContainer> tagContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();

            DisplayObjectGuid(objectIdLabel);
            RemoveNonexistentTagsFromContainers(tagContainers);
            SetupAddNewTagUI(newTagTextField, addNewTagButton, tagListContainer, tagContainers);
            SetupSearchableTagListPopup(addTagButton, tagListContainer, tagContainers);
            AddExistingTags(tagListContainer, tagContainers);
        }

        private void DisplayObjectGuid(Label objectIdLabel)
        {
            IEnumerable<ProcessSceneObject> componentsOutsideInScene = targets.OfType<ProcessSceneObject>().Where(t => !AssetUtility.IsComponentInScene(t));
            if (componentsOutsideInScene.Count() > 0)
            {
                objectIdLabel.SetEnabled(false);
                objectIdLabel.text = $"{UniqueIdDisplayName}: {AssetOnDiskText}";
            }
            else
            {
                IEnumerable<ProcessSceneObject> componentsInScene = targets.OfType<ProcessSceneObject>().Where(t => AssetUtility.IsComponentInScene(t));
                objectIdLabel.SetEnabled(componentsInScene.Count() == 1);
                objectIdLabel.text = componentsInScene.Count() == 1 ? $"{UniqueIdDisplayName}: {componentsInScene.FirstOrDefault()?.Guid.ToString()}" : $"{UniqueIdDisplayName}: {MultipleValuesSelectedText}";
            }
        }

        /// <summary>
        /// Clean up tags that might be deleted from the global SceneObjectTags registry
        /// </summary>
        /// <param name="tagContainers"></param>
        private void RemoveNonexistentTagsFromContainers(List<IGuidContainer> tagContainers)
        {
            foreach (IGuidContainer tagContainer in tagContainers)
            {
                List<Guid> tagsToRemove = new List<Guid>();

                foreach (Guid tagGuid in tagContainer.Guids)
                {
                    if (!SceneObjectTags.Instance.TagExists(tagGuid))
                    {
                        tagsToRemove.Add(tagGuid);
                    }
                }

                foreach (Guid tagGuid in tagsToRemove)
                {
                    tagContainer.RemoveGuid(tagGuid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)tagContainer);
                }
            }
        }

        private void SetupAddNewTagUI(TextField newTagTextField, Button addNewTagButton, VisualElement tagListContainer, List<IGuidContainer> tagContainers)
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

        private void AddExistingTags(VisualElement tagListContainer, List<IGuidContainer> tagContainers)
        {
            List<SceneObjectTags.Tag> usedTags = SceneObjectTags.Instance.Tags.Where(tag => tagContainers.Any(c => c.HasGuid(tag.Guid))).ToList();

            foreach (SceneObjectTags.Tag tag in usedTags)
            {
                bool variesAcrossSelection = tagContainers.Any(container => container.HasGuid(tag.Guid) == false);
                AddTagElement(tagListContainer, tag, variesAcrossSelection);
            }

            ValidateTagListContainer(tagListContainer);
        }

        private void SetupSearchableTagListPopup(Button addTagButton, VisualElement tagListContainer, List<IGuidContainer> tagContainers)
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
            const string noCustomTagsClassName = "noCustomTagsMessage";
            bool containsTag = tagListContainer.Q<VisualElement>("RemovableTagContainer") != null;
            VisualElement existingMessage = tagListContainer.Q<VisualElement>(className: noCustomTagsClassName);

            if (!containsTag && existingMessage == null)
            {
                VisualElement warning = noCustomTagsMessage.CloneTree();
                warning.AddToClassList(noCustomTagsClassName);
                tagListContainer.Add(warning);
            }
            else if (containsTag && existingMessage != null)
            {
                tagListContainer.Remove(existingMessage);
            }
        }


        private List<SceneObjectTags.Tag> GetAvailableTags()
        {
            List<IGuidContainer> tagContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();
            List<SceneObjectTags.Tag> availableTags = SceneObjectTags.Instance.Tags.Where(tag => !tagContainers.All(c => c.HasGuid(tag.Guid))).ToList();
            return availableTags;
        }

        private SceneObjectTags.Tag CreateTag(string newTagName, List<IGuidContainer> tagContainers)
        {
            Guid guid = Guid.NewGuid();
            Undo.RecordObject(SceneObjectTags.Instance, "Created tag");
            SceneObjectTags.Tag newTag = SceneObjectTags.Instance.CreateTag(newTagName, guid);
            AddTagToAll(tagContainers, newTag);
            return newTag;
        }

        private void AddTag(VisualElement tagListContainer, List<IGuidContainer> tagContainers, SceneObjectTags.Tag selectedTag)
        {
            AddTagToAll(tagContainers, selectedTag);
            AddTagElement(tagListContainer, selectedTag);
        }

        private void AddTagToAll(List<IGuidContainer> tagContainers, SceneObjectTags.Tag tag)
        {
            foreach (IGuidContainer container in tagContainers)
            {
                Undo.RecordObject((UnityEngine.Object)container, "Added tag");
                container.AddGuid(tag.Guid);
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

            tagElement.Q<Label>("Count").text = GetTagCount(tag);
            tagElement.Q<Button>("Button").clicked += () => RemoveTagElement(container, tagElement, tag);
            container.Add(tagElement);
            ValidateTagListContainer(container);

            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            EditorUtility.SetDirty(processSceneObject);
        }

        private string GetTagCount(SceneObjectTags.Tag tag)
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            if (AssetUtility.IsOnDisk(processSceneObject) || AssetUtility.IsEditingInPrefabMode(processSceneObject.gameObject))
            {
                return TagCountNotAvailableText;
            }
            return RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(tag.Guid).Count().ToString();
        }

        private void RemoveTagElement(VisualElement container, VisualElement tagElement, SceneObjectTags.Tag tag)
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.RemoveGuid(tag.Guid);
            container.Remove(tagElement);
            ValidateTagListContainer(container);

            EditorUtility.SetDirty(processSceneObject);
        }
    }
}
