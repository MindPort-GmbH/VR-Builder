using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI.Windows
{

    public class SearchableTagListPopup : PopupWindowContent
    {
        [SerializeField]
        private VisualTreeAsset searchableList = default;
        [SerializeField]
        private VisualTreeAsset listItem = default;
        private Vector2 windowSize;
        private Vector2 minWindowSize = new Vector2(200, 200); //Default size of PopupWindowContent
        private ScrollView tagScrollView;
        private List<SceneObjectTags.Tag> tags;
        private Action<SceneObjectTags.Tag> onItemSelected;

        public SearchableTagListPopup(Action<SceneObjectTags.Tag> onItemSelected, VisualTreeAsset searchableList, VisualTreeAsset tagListItem)
        {
            this.searchableList = searchableList;
            this.listItem = tagListItem;
            this.onItemSelected = onItemSelected;
            windowSize = minWindowSize;
        }

        public override Vector2 GetWindowSize()
        {
            return windowSize;
        }

        public override void OnGUI(Rect rect)
        {
            // intentionally left blank
        }

        public override void OnOpen()
        {
            // Initialize UI from UXML
            searchableList.CloneTree(editorWindow.rootVisualElement);

            //Set the style of the window depending on the editor skin
            VisualElement rootElement = editorWindow.rootVisualElement;
            rootElement.AddToClassList("searchableList");
            if (EditorGUIUtility.isProSkin)
                rootElement.AddToClassList("searchableList-dark");
            else
                rootElement.AddToClassList("searchableList-light");

            // Get references to UI elements
            ToolbarSearchField searchField = editorWindow.rootVisualElement.Q<ToolbarSearchField>("SearchTagField");
            tagScrollView = editorWindow.rootVisualElement.Q<ScrollView>("TagList");

            // Populate the list
            if (tags == null)
                tags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);
            PopulateList(tags, listItem);

            //Add event listener to the search field
            searchField.RegisterValueChangedCallback(evt => FilterList(evt.newValue));
            //Focus the search field after it is ready
            EditorApplication.delayCall += () => { searchField.Focus(); };
        }

        /// <summary>
        /// Set the with and or height of the window. 
        /// If the given value is smaller than the minimum size, the minimum size will be used.
        /// </summary>
        /// <param name="windowWith"></param>
        /// <param name="windowHeight"></param>
        public void SetWindowSize(float windowWith = -1, float windowHeight = -1)
        {
            this.windowSize = new Vector2(windowWith > minWindowSize.x ? windowWith : minWindowSize.x, windowHeight > minWindowSize.y ? windowHeight : minWindowSize.y);
        }

        /// <summary>
        /// Set the tags to be displayed in the list.
        /// </summary>
        /// <param name="availableTags"></param> 
        public void SetAvailableTags(List<SceneObjectTags.Tag> availableTags)
        {
            this.tags = availableTags.OrderBy(t => t.Label).ToList();
        }

        private void PopulateList(List<SceneObjectTags.Tag> availableTags, VisualTreeAsset listItem)
        {
            foreach (var tag in availableTags)
            {
                VisualElement item = listItem.CloneTree();
                item.Q<Label>("Label").text = tag.Label;

                // Set the style for hovering depending on the editor skin
                if (EditorGUIUtility.isProSkin)
                    item.AddToClassList("listItem-dark");
                else
                    item.AddToClassList("listItem-light");

                item.userData = tag;
                item.AddManipulator(new Clickable(() => OnLabelClick(item)));
                tagScrollView.Add(item);
            }
        }

        private void OnLabelClick(VisualElement clickedTag)
        {
            if (clickedTag.userData is SceneObjectTags.Tag tag)
            {
                onItemSelected?.Invoke(tag);
                editorWindow.Close();
            }
        }

        private void FilterList(string searchText)
        {
            searchText = searchText.ToLowerInvariant();
            foreach (VisualElement child in tagScrollView.Children())
            {
                if (child.Q<Label>("Label") is Label label)
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
