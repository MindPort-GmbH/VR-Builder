// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    /// <summary>
    /// UIToolkit port of <c>LockableObjectsDrawer</c>. Wraps the step's data in a
    /// <see cref="LockableObjectsCollection"/>, then renders the same three sections
    /// as the legacy inspector:
    ///   - Automatically + manually unlocked scene objects with per-property toggles
    ///   - Drop-target for adding new <see cref="ProcessSceneObject"/>s
    ///   - Group unlock list with per-property type toggles
    /// </summary>
    internal sealed class UnlockedObjectsTab : IStepInspectorPanel
    {
        public string Id => PanelIds.Unlocked;
        public GUIContent Label { get; } = new GUIContent("Unlocked Objects");

        public VisualElement BuildContent(IStepData step, IElementDrawerContext ctx)
        {
            ScrollView root = new ScrollView(ScrollViewMode.Vertical);
            root.AddToClassList("vrb-tab");
            root.AddToClassList("vrb-tab--unlocked");

            if (step is not Step.EntityData entityData)
            {
                root.Add(new Label("(this step type does not support unlocked objects)"));
                return root;
            }

            LockableObjectsCollection collection = new LockableObjectsCollection(entityData);

            Label sectionHeader = new Label("Automatically unlocked objects in this step");
            sectionHeader.AddToClassList("vrb-unlocked__section-header");
            root.Add(sectionHeader);

            foreach (ISceneObject sceneObject in collection.SceneObjects)
            {
                root.Add(BuildSceneObjectBlock(sceneObject, collection));
            }

            root.Add(BuildAddObjectDropTarget(collection));
            root.Add(BuildGroupSection(collection));

            return root;
        }

        public void Refresh() { }
        public void Dispose() { }

        // ───────── scene-object block ─────────

        private static VisualElement BuildSceneObjectBlock(ISceneObject sceneObject, LockableObjectsCollection collection)
        {
            VisualElement block = new VisualElement();
            block.AddToClassList("vrb-unlocked__object");

            VisualElement headerRow = new VisualElement();
            headerRow.AddToClassList("vrb-unlocked__object-header");
            headerRow.style.flexDirection = FlexDirection.Row;

            ObjectField objectField = new ObjectField
            {
                objectType = typeof(ProcessSceneObject),
                value = (ProcessSceneObject)sceneObject,
                allowSceneObjects = true
            };
            objectField.SetEnabled(false);
            objectField.style.flexGrow = 1f;
            headerRow.Add(objectField);

            bool canRemove = collection.IsUsedInAutoUnlock(sceneObject) == false;
            Button removeButton = new Button(() =>
            {
                if (collection.IsUsedInAutoUnlock(sceneObject)) return;
                collection.RemoveSceneObject(sceneObject);
                TriggerRebuild();
            })
            {
                text = Icons.Delete,
                tooltip = canRemove
                    ? "Remove this object from the manual unlock list"
                    : "Cannot remove — this object is automatically unlocked by a condition"
            };
            removeButton.SetEnabled(canRemove);
            removeButton.AddToClassList("vrb-unlocked__object-remove");
            headerRow.Add(removeButton);

            block.Add(headerRow);

            try
            {
                foreach (LockableProperty property in sceneObject.Properties.OfType<LockableProperty>())
                {
                    block.Add(BuildPropertyRow(property, collection));
                }
            }
            catch (MissingReferenceException)
            {
                // Same swallow as the legacy drawer — happens transiently when assemblies reload.
            }

            return block;
        }

        private static VisualElement BuildPropertyRow(LockableProperty property, LockableObjectsCollection collection)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-unlocked__property");
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginLeft = 16;

            bool isAutoUnlocked = collection.IsInAutoUnlockList(property);
            bool isFlagged = isAutoUnlocked || collection.IsInManualUnlockList(property);

            Toggle toggle = new Toggle
            {
                value = isFlagged,
                tooltip = isAutoUnlocked
                    ? "Locked on — this property is automatically unlocked by a condition"
                    : "Toggle manual unlock for this property"
            };
            toggle.SetEnabled(isAutoUnlocked == false);
            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (isAutoUnlocked) return;

                if (evt.newValue) collection.Add(property);
                else collection.Remove(property);

                TriggerRebuild();
            });
            row.Add(toggle);

            Label label = new Label(property.GetType().Name);
            label.style.marginLeft = 4;
            row.Add(label);

            return row;
        }

        // ───────── add new object via drop ─────────

        private static VisualElement BuildAddObjectDropTarget(LockableObjectsCollection collection)
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("vrb-unlocked__add-object");
            container.style.marginTop = 8;

            Label hint = new Label("Drag a Process Scene Object here to add it:");
            hint.AddToClassList("vrb-unlocked__add-hint");
            container.Add(hint);

            ObjectField field = new ObjectField
            {
                objectType = typeof(ProcessSceneObject),
                allowSceneObjects = true,
                value = null
            };
            field.AddToClassList("vrb-unlocked__add-field");
            field.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is ProcessSceneObject pso)
                {
                    collection.AddSceneObject(pso);
                    field.SetValueWithoutNotify(null);
                    TriggerRebuild();
                }
            });
            container.Add(field);

            return container;
        }

        // ───────── groups section ─────────

        private static VisualElement BuildGroupSection(LockableObjectsCollection collection)
        {
            VisualElement section = new VisualElement();
            section.AddToClassList("vrb-unlocked__groups");
            section.style.marginTop = 12;

            Label header = new Label("Groups to unlock");
            header.AddToClassList("vrb-unlocked__section-header");
            section.Add(header);

            foreach (Guid groupGuid in collection.TagsToUnlock.ToList())
            {
                section.Add(BuildGroupBlock(groupGuid, collection));
            }

            Button addGroupButton = new Button(() => OpenGroupPicker(collection))
            {
                text = "+ Add group to unlock list",
                tooltip = "Pick a Scene Object Group whose properties should be unlocked for this step"
            };
            addGroupButton.AddToClassList("vrb-add-button");
            section.Add(addGroupButton);

            return section;
        }

        private static VisualElement BuildGroupBlock(Guid groupGuid, LockableObjectsCollection collection)
        {
            string title = SceneObjectGroups.Instance.GetLabel(groupGuid);

            CollapsibleItem block = new CollapsibleItem(
                title: string.IsNullOrEmpty(title) ? "(unnamed group)" : title,
                onDelete: () =>
                {
                    collection.RemoveGroup(groupGuid);
                    TriggerRebuild();
                },
                gripTooltip: Tooltips.Grip,
                deleteTooltip: Tooltips.UnlockedRemoveGroup,
                startExpanded: false,
                stateKey: groupGuid);
            block.AddToClassList("vrb-item--group");

            foreach (Type propertyType in PropertyReflectionHelper.ExtractFittingPropertyType<LockableProperty>(typeof(LockableProperty)))
            {
                Toggle toggle = new Toggle(propertyType.Name)
                {
                    value = collection.IsPropertyEnabledForGroup(groupGuid, propertyType)
                };
                toggle.style.marginLeft = 8;

                Type capturedType = propertyType;
                toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue) collection.AddPropertyToGroup(groupGuid, capturedType);
                    else collection.RemovePropertyFromGroup(groupGuid, capturedType);

                    TriggerRebuild();
                });
                block.Body.Add(toggle);
            }

            return block;
        }

        private static void OpenGroupPicker(LockableObjectsCollection collection)
        {
            GenericMenu menu = new GenericMenu();
            IEnumerable<SceneObjectGroups.SceneObjectGroup> available = SceneObjectGroups.Instance.Groups
                .Where(group => collection.TagsToUnlock.Contains(group.Guid) == false)
                .OrderBy(g => g.Label);

            int count = 0;
            foreach (SceneObjectGroups.SceneObjectGroup group in available)
            {
                Guid captured = group.Guid;
                menu.AddItem(new GUIContent(group.Label ?? "(unnamed)"), false, () =>
                {
                    collection.AddGroup(captured);
                    TriggerRebuild();
                });
                count++;
            }

            if (count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No groups available"));
            }

            menu.ShowAsContext();
        }

        // ───────── shared ─────────

        private static void TriggerRebuild()
        {
            Windows.StepSelectionService.NotifyStepModified();
        }
    }
}
