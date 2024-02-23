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

    /// <summary>
    /// Popup window that displays a searchable list of all VR Builder <seealso cref="SceneObjectTags"/>.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for displaying a a searchable list of 
    /// all VR Builder <seealso cref="SceneObjectTags"/> in a popup window,
    /// where the tags can be filtered based on a search query. The class supports partial
    /// word matching in the search.
    /// </remarks>
    public class SearchableTagListPopup : PopupWindowContent
    {
        /// <summary>
        /// Root VisualTreeAsset for the searchable list.
        /// </summary>
        [SerializeField]
        private VisualTreeAsset searchableList = default;

        /// <summary>
        /// VisualTreeAsset for the individual list items.
        /// </summary>
        [SerializeField]
        private VisualTreeAsset listItem = default;

        /// <summary>
        /// The size of the popup window.
        /// </summary>
        private Vector2 windowSize;

        /// <summary>
        /// The minimum size of the popup window.
        /// </summary>
        private Vector2 minWindowSize = new Vector2(200, 200);

        /// <summary>
        /// The ScrollView containing the list of tags.
        /// </summary>
        private ScrollView tagScrollView;

        /// <summary>
        /// The list of tags which will be used.
        /// </summary>
        private List<SceneObjectTags.Tag> tags;

        /// <summary>
        /// Callback to invoke when a tag is selected.
        /// </summary>
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
            searchField.RegisterValueChangedCallback(evt => FilterListByPartialMatch(evt.newValue));
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
            windowSize = new Vector2(windowWith > minWindowSize.x ? windowWith : minWindowSize.x, windowHeight > minWindowSize.y ? windowHeight : minWindowSize.y);
        }

        /// <summary>
        /// Set the tags to be displayed in the list.
        /// </summary>
        /// <param name="availableTags"></param> 
        public void SetAvailableTags(List<SceneObjectTags.Tag> availableTags)
        {
            tags = availableTags.OrderBy(t => t.Label).ToList();
        }

        /// <summary>
        /// Populates the ScrollView with a list of <seealso cref="SceneObjectTags.Tag"/>.
        /// </summary>
        /// <remarks>
        /// This method takes a list of tags and a VisualTreeAsset representing the list item template.
        /// It iterates through each tag, clones the template, sets the tag's label, and adds the item
        /// to the ScrollView. Each item is styled depending on the editor skin for hover highting 
        /// and configured with a click event handler.
        /// </remarks>
        /// <param name="availableTags">The list of <seealso cref="SceneObjectTags.Tag"/> to be displayed in the list.</param>
        /// <param name="listItem">The VisualTreeAsset used as a template for each list item.</param>
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

        /// <summary>
        /// Filters the list of items based on the provided search text.
        /// </summary>
        /// <remarks>
        /// This method allows for partial word matches in the search. It splits the search text into individual words
        /// and checks if each word is contained within each item's label text. The search is case-insensitive and
        /// considers each space-separated part of the search text independently.
        ///
        /// For example, searching for "Cube 1" will match an item with a label "Cube1Red" as well as "Cube 12",
        /// as both contain the parts "Cube" and "1".
        /// </remarks>
        /// <param name="searchText">The text to search for. It's split into parts separated by spaces.</param>
        private void FilterListByPartialMatch(string searchText)
        {
            searchText = searchText.ToLowerInvariant();
            string[] searchParts = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (VisualElement child in tagScrollView.Children())
            {
                if (child.Q<Label>("Label") is Label label)
                {
                    bool isMatch = true;
                    if (searchParts.Length > 0)
                    {
                        string labelLower = label.text.ToLowerInvariant();
                        isMatch = searchParts.All(part => labelLower.Contains(part));
                    }

                    child.style.display = isMatch ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
    }
}
