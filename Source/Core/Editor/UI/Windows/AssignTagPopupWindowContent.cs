using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{

    public class AssignTagPopupWindowContent : PopupWindowContent
    {
        [SerializeField]
        private VisualTreeAsset searchableList = default;
        [SerializeField]
        private VisualTreeAsset listItem = default;
        private Action<SceneObjectTags.Tag> onItemSelected;
        private ScrollView tagList;

        private Vector2 windowSize;
        private Vector2 minWindowSize = new Vector2(225, 200);


        public AssignTagPopupWindowContent(Action<SceneObjectTags.Tag> onItemSelected, VisualTreeAsset searchableDropdown, VisualTreeAsset tagListItem)
        {
            this.searchableList = searchableDropdown;
            this.listItem = tagListItem;
            this.onItemSelected = onItemSelected;
            windowSize = minWindowSize;
        }

        public override Vector2 GetWindowSize()
        {
            return windowSize;
        }

        public void SetWindowSize(float windowWith = -1, float windowHeight = -1)
        {
            this.windowSize = new Vector2(windowWith > minWindowSize.x ? windowWith : minWindowSize.x, windowHeight > minWindowSize.y ? windowHeight : minWindowSize.y);
        }

        public override void OnGUI(Rect rect)
        {
            // intentionally left blank
        }

        public override void OnOpen()
        {
            // Initialize UI from UXML
            searchableList.CloneTree(editorWindow.rootVisualElement);

            // Get references to UI elements
            ToolbarSearchField searchField = editorWindow.rootVisualElement.Q<ToolbarSearchField>("SearchTagField");
            tagList = editorWindow.rootVisualElement.Q<ScrollView>("TagList");

            // Populate the list
            List<SceneObjectTags.Tag> availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);
            PopulateList(availableTags, listItem);

            //Add event listener to the search field
            searchField.RegisterValueChangedCallback(evt => FilterList(evt.newValue));
        }

        public override void OnClose()
        {
            // Clean up or handle closing logic here
        }

        private void PopulateList(List<SceneObjectTags.Tag> availableTags, VisualTreeAsset tagListItem)
        {
            foreach (var tag in availableTags)
            {
                VisualElement listItem = tagListItem.CloneTree();
                listItem.Q<Label>("TagLabel").text = tag.Label;
                
                // Set the style for hovering depending on the editor skin
                if (EditorGUIUtility.isProSkin)
                    listItem.AddToClassList("listItem-dark");
                else
                    listItem.AddToClassList("listItem-light");
                
                listItem.userData = tag;
                listItem.AddManipulator(new Clickable(() => OnLabelClick(listItem)));
                tagList.Add(listItem);
            }
        }

        private void OnLabelClick(VisualElement clickedTag)
        {
            if (clickedTag.userData is SceneObjectTags.Tag tag)
            {
                onItemSelected?.Invoke(tag);
                editorWindow.Close(); // Close the popup window
            }
        }

        private void FilterList(string searchText)
        {
            searchText = searchText.ToLowerInvariant();
            foreach (VisualElement child in tagList.Children())
            {
                if (child.Q<Label>("TagLabel") is Label label)
                {
                    bool isMatch = true;
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        isMatch = label.text.ToLowerInvariant().Contains(searchText);
                    }

                    if (isMatch)
                        child.style.display = DisplayStyle.Flex;
                    else
                        child.style.display = DisplayStyle.None;
                }
            }
        }
    }
}
