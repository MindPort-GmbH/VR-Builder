// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Views;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Unity;

namespace VRBuilder.Core.Editor.UI
{
    /// <summary>
    /// Custom editor for <see cref="ProcessSceneObject"/>.
    /// </summary>
    [CustomEditor(typeof(ProcessSceneObject))]
    [CanEditMultipleObjects]
    public class ProcessSceneObjectEditor : UnityEditor.Editor
    {
        public const string UniqueIdDisplayName = "Object ID";
        public const string AssetOnDiskText = "[Asset on disk]";
        public const string MultipleValuesSelectedText = "[Multiple values selected]";

        [SerializeField]
        private VisualTreeAsset manageGroupsPanel;
        [SerializeField]
        private VisualTreeAsset removableGroup;
        [SerializeField]
        private VisualTreeAsset noGroupsMessage;
        [SerializeField]
        private VisualTreeAsset searchableList;
        [SerializeField]
        private VisualTreeAsset groupListItem;

        private TextField newGroupTextField;
        private Button addNewGroupButton;
        private Button addGroupButton;
        private VisualElement groupListContainer;
        private Label objectIdLabel;

        private void OnEnable()
        {
            EditorUtils.CheckVisualTreeAssets(nameof(ProcessSceneObjectEditor), new List<VisualTreeAsset>() { manageGroupsPanel, removableGroup, noGroupsMessage, searchableList, groupListItem });
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.ObjectIdChanged += OnUniqueIdChanged;
        }

        private void OnDisable()
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.ObjectIdChanged -= OnUniqueIdChanged;
        }

        private void OnUniqueIdChanged(object sender, UniqueIdChangedEventArgs e)
        {
            if (objectIdLabel != null)
            {
                DisplayObjectGuid(objectIdLabel);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            manageGroupsPanel.CloneTree(root);
            if (RuntimeConfigurator.Exists)
            {
                SetupGroupManagement(root);
            }
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

        private void SetupGroupManagement(VisualElement root)
        {
            // Retrieve the necessary elements and containers from the cloned tree
            newGroupTextField = root.Q<TextField>("NewGroupTextField");
            addNewGroupButton = root.Q<Button>("NewGroupButton");
            addGroupButton = root.Q<Button>("AddGroupButton");
            groupListContainer = root.Q<VisualElement>("GroupList");
            objectIdLabel = root.Q<Label>("ObjectId");

            List<IGuidContainer> groupContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();

            if (AdvancedSettings.Instance.ShowExpertInfo == false)
            {
                objectIdLabel.RemoveFromHierarchy();
            }
            else
            {
                DisplayObjectGuid(objectIdLabel);
            }

            RemoveNonexistentGroupFromContainers(groupContainers);
            SetupAddNewGroupUI(newGroupTextField, addNewGroupButton, groupListContainer, groupContainers);
            SetupSearchableGroupListPopup(addGroupButton, groupListContainer, groupContainers);
            AddExistingGroups(groupListContainer, groupContainers);
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
        /// Clean up groups that might be deleted from the global <seealso cref="SceneObjectRegistry"/>
        /// </summary>
        /// <param name="groupContainers"></param>
        private void RemoveNonexistentGroupFromContainers(List<IGuidContainer> groupContainers)
        {
            foreach (IGuidContainer groupContainer in groupContainers)
            {
                List<Guid> groupsToRemove = new List<Guid>();

                foreach (Guid groupGuid in groupContainer.Guids)
                {
                    if (!SceneObjectGroups.Instance.GroupExists(groupGuid))
                    {
                        groupsToRemove.Add(groupGuid);
                    }
                }

                foreach (Guid groupGuid in groupsToRemove)
                {
                    groupContainer.RemoveGuid(groupGuid);
                    PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)groupContainer);
                }
            }
        }

        private void SetupAddNewGroupUI(TextField newGroupTextField, Button addNewGroupButton, VisualElement groupListContainer, List<IGuidContainer> groupContainers)
        {
            EvaluateNewGroupName(newGroupTextField.text, addNewGroupButton);
            newGroupTextField.RegisterValueChangedCallback(evt => EvaluateNewGroupName(evt.newValue, addNewGroupButton));

            addNewGroupButton.clicked += () =>
            {
                SceneObjectGroups.SceneObjectGroup newGroup = CreateGroup(newGroupTextField.text, groupContainers);
                AddGroupElement(groupListContainer, newGroup);
                newGroupTextField.value = "";
                EditorUtility.SetDirty(SceneObjectGroups.Instance);
            };
        }

        private void AddExistingGroups(VisualElement groupListContainer, List<IGuidContainer> groupContainers)
        {
            List<SceneObjectGroups.SceneObjectGroup> usedGroup = SceneObjectGroups.Instance.Groups.Where(group => groupContainers.Any(c => c.HasGuid(group.Guid))).ToList();

            foreach (SceneObjectGroups.SceneObjectGroup group in usedGroup)
            {
                bool elementIsUniqueIdDisplayName = groupContainers.Any(container => container.HasGuid(group.Guid) == false);
                AddGroupElement(groupListContainer, group, elementIsUniqueIdDisplayName);
            }

            ValidateGroupListContainer(groupListContainer);
        }

        private void SetupSearchableGroupListPopup(Button addGroupButton, VisualElement groupListContainer, List<IGuidContainer> groupContainers)
        {
            Action<SceneObjectGroups.SceneObjectGroup> onItemSelected = (SceneObjectGroups.SceneObjectGroup selectedGroup) =>
            {
                AddGroup(groupListContainer, groupContainers, selectedGroup);
            };

            addGroupButton.clicked += () =>
            {
                SearchableGroupListPopup content = new SearchableGroupListPopup(onItemSelected, searchableList, groupListItem);
                content.SetContext(AssetUtility.IsInPreviewContext(((ProcessSceneObject)target).gameObject));
                content.SetAvailableGroups(GetAvailableGroups());
                content.SetWindowSize(windowWith: addGroupButton.resolvedStyle.width);

                UnityEditor.PopupWindow.Show(addGroupButton.worldBound, content);
            };

            SearchableGroupListPopup content = new SearchableGroupListPopup(onItemSelected, searchableList, groupListItem);
        }

        private void ValidateGroupListContainer(VisualElement groupListContainer)
        {
            const string noGroupsClassName = "noGroupsMessage";
            bool containsGroup = groupListContainer.Q<VisualElement>("RemovableGroupContainer") != null;
            VisualElement existingMessage = groupListContainer.Q<VisualElement>(className: noGroupsClassName);

            if (!containsGroup && existingMessage == null)
            {
                VisualElement warning = noGroupsMessage.CloneTree();
                warning.AddToClassList(noGroupsClassName);
                groupListContainer.Add(warning);
            }
            else if (containsGroup && existingMessage != null)
            {
                groupListContainer.Remove(existingMessage);
            }
        }


        private List<SceneObjectGroups.SceneObjectGroup> GetAvailableGroups()
        {
            List<IGuidContainer> groupContainers = targets.Where(t => t is IGuidContainer).Cast<IGuidContainer>().ToList();
            List<SceneObjectGroups.SceneObjectGroup> availableGroups = SceneObjectGroups.Instance.Groups.Where(group => !groupContainers.All(c => c.HasGuid(group.Guid))).ToList();
            return availableGroups;
        }

        private SceneObjectGroups.SceneObjectGroup CreateGroup(string newGroupName, List<IGuidContainer> groupContainers)
        {
            Guid guid = Guid.NewGuid();
            Undo.RecordObject(SceneObjectGroups.Instance, "Created group");
            SceneObjectGroups.SceneObjectGroup newGroup = SceneObjectGroups.Instance.CreateGroup(newGroupName, guid);
            AddGroupToAll(groupContainers, newGroup);
            return newGroup;
        }

        private void AddGroup(VisualElement groupListContainer, List<IGuidContainer> groupContainers, SceneObjectGroups.SceneObjectGroup selectedGroup)
        {
            AddGroupToAll(groupContainers, selectedGroup);
            AddGroupElement(groupListContainer, selectedGroup);
        }

        private void AddGroupToAll(List<IGuidContainer> groupContainers, SceneObjectGroups.SceneObjectGroup group)
        {
            foreach (IGuidContainer container in groupContainers)
            {
                Undo.RecordObject((UnityEngine.Object)container, "Added group");
                container.AddGuid(group.Guid);
            }

            foreach (UnityEngine.Object unityObject in groupContainers.Where(container => container is UnityEngine.Object).Cast<UnityEngine.Object>())
            {
                EditorUtility.SetDirty(unityObject);
            }
        }

        private static void EvaluateNewGroupName(string newGroup, Button addNewGroupButton)
        {
            addNewGroupButton.SetEnabled(SceneObjectGroups.Instance.CanCreateGroup(newGroup));
        }

        private void AddGroupElement(VisualElement container, SceneObjectGroups.SceneObjectGroup group, bool elementIsUniqueIdDisplayName = false)
        {
            VisualElement removableGroupListItem = removableGroup.CloneTree();
            VisualElement groupListElement = groupListItem.CloneTree();
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;

            bool isPreviewInContext = AssetUtility.IsInPreviewContext(((ProcessSceneObject)target).gameObject);
            IEnumerable<ISceneObject> referencedSceneObjects = RuntimeConfigurator.Configuration?.SceneObjectRegistry?.GetObjects(group.Guid);

            GroupListItem.FillGroupListItem(groupListElement, group.Label, isPreviewInContext: isPreviewInContext,
                referencedSceneObjects: referencedSceneObjects, elementIsUniqueIdDisplayName: elementIsUniqueIdDisplayName);

            VisualElement removableGroupContainer = removableGroupListItem.Q<VisualElement>("RemovableGroupContainer");
            removableGroupListItem.Q<Button>("Button").clicked += () => RemoveGroupElement(container, removableGroupListItem, group);
            removableGroupContainer.Add(groupListElement);

            container.Add(removableGroupListItem);
            ValidateGroupListContainer(container);

            EditorUtility.SetDirty(processSceneObject);
        }


        private void RemoveGroupElement(VisualElement container, VisualElement groupElement, SceneObjectGroups.SceneObjectGroup group)
        {
            ProcessSceneObject processSceneObject = (ProcessSceneObject)target;
            processSceneObject.RemoveGuid(group.Guid);
            container.Remove(groupElement);
            ValidateGroupListContainer(container);

            EditorUtility.SetDirty(processSceneObject);
        }
    }
}
