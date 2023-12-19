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

        private Vector2 windowSize = new Vector2(250, 150);

        public AssignTagPopupWindowContent(Action<SceneObjectTags.Tag> onItemSelected, VisualTreeAsset searchableDropdown, VisualTreeAsset tagListItem)
        {
            this.searchableList = searchableDropdown;
            this.listItem = tagListItem;
            this.onItemSelected = onItemSelected;
        }

        public override Vector2 GetWindowSize()
        {
            return windowSize;
        }

        public override void OnGUI(Rect rect)
        {
            // intentionally left blank

            // // Create a root VisualElement
            // VisualElement root = new VisualElement();
            // root.style.flexGrow = 1;
            // root.style.width = rect.width;
            // root.style.height = rect.height;

            // // Instantiate UXML
            // VisualElement assignTagWindowUXML = visualTreeAsset.Instantiate();
            // root.Add(assignTagWindowUXML);

            // var searchField = root.Q<ToolbarSearchField>("SearchTagField");
            // tagList = root.Q<ScrollView>("TagList");

            // // Populate the list and add event listeners
            // PopulateList();
            // searchField.RegisterValueChangedCallback(evt => FilterList(evt.newValue));

            // // Add the root to the IMGUIContainer to render UI Toolkit elements
            // var container = new IMGUIContainer(() => root.DrawHierarchy(new Rect(0, 0, rect.width, rect.height)));
            // container.style.flexGrow = 1;
            // container.OnGUI();
        }

        public override void OnOpen()
        {
            // Initialize UI from UXML
            // Initialize UI from UXML
            searchableList.CloneTree(editorWindow.rootVisualElement);

            // Get references to UI elements
            var searchField = editorWindow.rootVisualElement.Q<ToolbarSearchField>("SearchTagField");
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
                listItem.userData = tag;
                listItem.AddManipulator(new Clickable(() => OnLabelClick(listItem)));
                tagList.Add(listItem);
            }
        }

        private void OnLabelClick(VisualElement clickedTag)
        {
            Debug.Log("Clicked on: " + clickedTag.Q<Label>("TagLabel").text);
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
                 
                    if(isMatch)
                        child.style.display = DisplayStyle.Flex;
                    else
                        child.style.display = DisplayStyle.None;
                }
            }
        }
    }
}
