using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UI.Views;

namespace VRBuilder.Editor.UI.Windows
{
    public class SceneReferencesEditorPopup : PopupWindowContent
    {
        private ProcessSceneReferenceBase reference;

        private Action<object> changeValueCallback;

        /// <summary>
        /// The size of the popup window.
        /// </summary>
        private Vector2 windowSize;

        /// <summary>
        /// The minimum size of the popup window.
        /// </summary>
        private Vector2 minWindowSize = new Vector2(200, 200);

        public SceneReferencesEditorPopup(ProcessSceneReferenceBase reference, Action<object> changeValueCallback)
        {
            this.reference = reference;
            this.changeValueCallback = changeValueCallback;
        }

        public override void OnGUI(Rect rect)
        {
            // intentionally left blank
        }

        public override void OnOpen()
        {
            // Load the UXML
            VisualTreeAsset sceneReferencesList = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SceneReferencesList);
            VisualTreeAsset sceneReferencesGroupItem = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SceneReferencesGroupItem);
            VisualTreeAsset sceneReferencesObjectItem = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SceneReferencesObjectItem);
            sceneReferencesList.CloneTree(editorWindow.rootVisualElement);

            VisualElement rootElement = editorWindow.rootVisualElement;
            // Set up the ScrollView
            ScrollView scrollView = rootElement.Q<ScrollView>("ScrollView");
            foreach (Guid guidToDisplay in reference.Guids)
            {
                VisualElement objectContainer = AddGroup(guidToDisplay, scrollView, sceneReferencesGroupItem, changeValueCallback);

                IEnumerable<ISceneObject> processSceneObjectsWithGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);
                foreach (ISceneObject sceneObject in processSceneObjectsWithGroup)
                {
                    VisualElement groupItem = sceneReferencesObjectItem.CloneTree();
                    groupItem.Q<Label>("objectLabel").text = sceneObject.GameObject.name;
                    objectContainer.Add(groupItem);

                    Button removeGroupButton = groupItem.Q<Button>("showButton");
                    removeGroupButton.clicked += () =>
                    {
                        EditorGUIUtility.PingObject(sceneObject.GameObject);
                    };
                }

                // Close button setup
                Button closeButton = rootElement.Q<Button>("CloseButton");
                closeButton.clicked += () => editorWindow.Close();
            }
        }

        /// <summary>
        /// Set the with and or height of the window. 
        /// If the given value is smaller than the minimum size, the minimum size will be used.
        /// </summary>
        /// <param name="windowWith"></param>
        /// <param name="windowHeight"></param>
        public void SetWindowSize(float windowWith = -1, float windowHeight = -1)
        {
            windowSize = new Vector2(windowWith > minWindowSize.x ? windowWith : minWindowSize.x, windowHeight > minWindowSize.y ? windowHeight : minWindowSize.y);
        }

        public override Vector2 GetWindowSize()
        {
            return windowSize;
        }

        private VisualElement AddGroup(Guid guidToDisplay, ScrollView scrollView, VisualTreeAsset sceneReferencesGroupItem, Action<object> changeValueCallback)
        {
            string label;

            SceneObjectGroups.SceneObjectGroup group;
            if (SceneObjectGroups.Instance.TryGetGroup(guidToDisplay, out group))
            {
                label = $"Group: {group.Label}";
            }
            else
            {
                label = SceneObjectGroups.UniqueGuidName;
            }

            ISceneObjectRegistry registry = RuntimeConfigurator.Configuration.SceneObjectRegistry;
            if (registry.ContainsGuid(guidToDisplay) == false && group == null)
            {
                label = $"{SceneObjectGroups.GuidNotRegisteredText} - {guidToDisplay}.";
            }

            VisualElement groupItem = sceneReferencesGroupItem.CloneTree();
            groupItem.Q<Label>("groupLabel").text = label;
            scrollView.Add(groupItem);

            Button selectGroupButton = groupItem.Q<Button>("selectButton");
            selectGroupButton.clicked += () =>
            {
                // Select all game objects with the group in the Hierarchy
                IEnumerable<ISceneObject> processSceneObjectsWithGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);
                Selection.objects = processSceneObjectsWithGroup.Select(processSceneObject => processSceneObject.GameObject).ToArray();
            };

            Button removeGroupButton = groupItem.Q<Button>("removeButton");
            removeGroupButton.clicked += () =>
            {
                reference.RemoveGuid(guidToDisplay);
                groupItem.RemoveFromHierarchy();
                changeValueCallback(reference);
            };

            VisualElement objectContainer = groupItem.Q<VisualElement>("objectContainer");
            return objectContainer;
        }
    }
}
