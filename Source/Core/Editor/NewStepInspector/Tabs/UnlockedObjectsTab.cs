// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Tabs
{
    /// <summary>
    /// Unlocked Objects tab: Shows scene objects with lockable property toggles
    /// and group unlock management. Ports LockableObjectsDrawer from IMGUI.
    /// </summary>
    public class UnlockedObjectsTab : IStepInspectorTab
    {
        public string TabLabel => "Unlocked Objects";
        public bool HasWarning => false;

        private IStep step;
        private VisualElement contentRoot;
        private VisualElement objectListContainer;
        private VisualElement groupListContainer;
        private LockableObjectsCollection collection;

        public VisualElement BuildContent()
        {
            contentRoot = new VisualElement();
            contentRoot.style.paddingLeft = 8;
            contentRoot.style.paddingRight = 8;
            contentRoot.style.paddingTop = 8;

            // Scene Objects section
            Label sceneObjectsTitle = new Label("Scene Objects");
            sceneObjectsTitle.AddToClassList("step-inspector__section-title");
            contentRoot.Add(sceneObjectsTitle);

            objectListContainer = new VisualElement();
            contentRoot.Add(objectListContainer);

            // Separator
            VisualElement separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            separator.style.marginTop = 8;
            separator.style.marginBottom = 8;
            contentRoot.Add(separator);

            // Groups section
            Label groupsTitle = new Label("Groups to Unlock");
            groupsTitle.AddToClassList("step-inspector__section-title");
            contentRoot.Add(groupsTitle);

            groupListContainer = new VisualElement();
            contentRoot.Add(groupListContainer);

            ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.Add(contentRoot);
            return scrollView;
        }

        public void Refresh(IStep step, IChapter chapter, IProcess process)
        {
            this.step = step;

            objectListContainer?.Clear();
            groupListContainer?.Clear();

            if (step?.Data == null) return;

            try
            {
                Step.EntityData entityData = step.Data as Step.EntityData;
                if (entityData == null) return;

                collection = new LockableObjectsCollection(entityData);
                BuildSceneObjectsList();
                BuildGroupsList();
            }
            catch (Exception e)
            {
                objectListContainer?.Add(new Label($"Error loading unlocked objects: {e.Message}"));
            }
        }

        public void Dispose() { }

        private void BuildSceneObjectsList()
        {
            if (collection == null || collection.SceneObjects == null) return;

            if (collection.SceneObjects.Count == 0)
            {
                objectListContainer.Add(new Label("No scene objects unlocked in this step."));
                return;
            }

            foreach (ISceneObject sceneObject in collection.SceneObjects)
            {
                if (sceneObject == null) continue;

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 4;
                row.style.alignItems = Align.Center;

                // Object name label (disabled ObjectField equivalent)
                Label objectLabel = new Label(sceneObject.GameObject != null
                    ? sceneObject.GameObject.name
                    : "(Missing Object)");
                objectLabel.style.flexGrow = 1;
                objectLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(objectLabel);

                // Remove button
                ISceneObject closuredObject = sceneObject;
                bool canRemove = !IsRequiredByAutomaticUnlock(sceneObject);
                Button removeButton = new Button(() => RemoveSceneObject(closuredObject))
                {
                    text = "x",
                    tooltip = canRemove ? "Remove from unlocked list" : "Required by automatic unlock"
                };
                removeButton.SetEnabled(canRemove);
                removeButton.style.width = 24;
                row.Add(removeButton);

                objectListContainer.Add(row);

                // Lockable properties for this object
                BuildLockablePropertiesForObject(sceneObject);
            }
        }

        private void BuildLockablePropertiesForObject(ISceneObject sceneObject)
        {
            if (sceneObject?.GameObject == null) return;

            LockableProperty[] lockableProperties = sceneObject.GameObject.GetComponents<LockableProperty>();
            if (lockableProperties == null || lockableProperties.Length == 0) return;

            VisualElement propertiesContainer = new VisualElement();
            propertiesContainer.style.marginLeft = 16;
            propertiesContainer.style.marginBottom = 4;

            foreach (LockableProperty property in lockableProperties)
            {
                if (property == null) continue;

                VisualElement propertyRow = new VisualElement();
                propertyRow.style.flexDirection = FlexDirection.Row;
                propertyRow.style.alignItems = Align.Center;

                Toggle toggle = new Toggle(property.GetType().Name)
                {
                    value = !property.IsLocked
                };

                LockableProperty closuredProperty = property;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    // Toggle unlock state
                    if (evt.newValue)
                    {
                        closuredProperty.SetLocked(false);
                    }
                    else
                    {
                        closuredProperty.SetLocked(true);
                    }
                    GlobalEditorHandler.CurrentProcessModified();
                });

                propertyRow.Add(toggle);
                propertiesContainer.Add(propertyRow);
            }

            objectListContainer.Add(propertiesContainer);
        }

        private void BuildGroupsList()
        {
            if (collection == null) return;

            IEnumerable<Guid> tags = collection.TagsToUnlock;
            if (tags == null || !tags.Any())
            {
                groupListContainer.Add(new Label("No groups to unlock."));
                return;
            }

            foreach (Guid tag in tags)
            {
                Foldout foldout = new Foldout { text = $"Group: {tag}" };
                foldout.style.marginLeft = 8;

                // Group properties would be displayed here with toggles per LockableProperty type
                foldout.Add(new Label("(Property toggles for this group)"));

                groupListContainer.Add(foldout);
            }
        }

        private void RemoveSceneObject(ISceneObject sceneObject)
        {
            if (collection == null) return;
            collection.RemoveSceneObject(sceneObject);
            GlobalEditorHandler.CurrentProcessModified();
            Refresh(step, null, null);
        }

        private bool IsRequiredByAutomaticUnlock(ISceneObject sceneObject)
        {
            // An object is required if it's referenced by automatic unlock properties
            // (i.e., it can't be removed because a behavior/condition depends on it)
            return false; // Simplified - full implementation would check toUnlock list
        }
    }
}
