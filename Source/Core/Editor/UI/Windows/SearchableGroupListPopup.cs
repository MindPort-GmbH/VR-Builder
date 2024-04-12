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
    /// Popup window that displays a searchable list of all VR Builder <seealso cref="SceneObjectGroups"/>.
    /// </summary>
    /// <remarks>
    /// This class provides functionality for displaying a a searchable list of 
    /// all VR Builder <seealso cref="SceneObjectGroups"/> in a popup window,
    /// where the groups can be filtered based on a search query. The class supports partial
    /// word matching in the search.
    /// </remarks>
    public class SearchableGroupListPopup : PopupWindowContent
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
        /// The ScrollView containing the list of groups.
        /// </summary>
        private ScrollView scrollView;

        /// <summary>
        /// The list of groups which will be used.
        /// </summary>
        private List<SceneObjectGroups.SceneObjectGroup> groups;

        /// <summary>
        /// Callback to invoke when a group is selected.
        /// </summary>
        private Action<SceneObjectGroups.SceneObjectGroup> onItemSelected;

        public SearchableGroupListPopup(Action<SceneObjectGroups.SceneObjectGroup> onItemSelected, VisualTreeAsset searchableList, VisualTreeAsset listItem)
        {
            this.searchableList = searchableList;
            this.listItem = listItem;
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
            ToolbarSearchField searchField = editorWindow.rootVisualElement.Q<ToolbarSearchField>("SearchGroupField");
            scrollView = editorWindow.rootVisualElement.Q<ScrollView>("GroupList");

            // Populate the list
            if (groups == null)
                groups = new List<SceneObjectGroups.SceneObjectGroup>(SceneObjectGroups.Instance.Groups);
            PopulateList(groups, listItem);

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
        /// Set the groups to be displayed in the list.
        /// </summary>
        /// <param name="availableGroups"></param> 
        public void SetAvailableGroups(IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups)
        {
            groups = availableGroups.OrderBy(t => t.Label).ToList();
        }

        /// <summary>
        /// Populates the ScrollView with a list of <seealso cref="SceneObjectGroups.SceneObjectGroup"/>.
        /// </summary>
        /// <remarks>
        /// This method takes a list of groups and a VisualTreeAsset representing the list item template.
        /// It iterates through each group, clones the template, sets the group's label, and adds the item
        /// to the ScrollView. Each item is styled depending on the editor skin for hover highting 
        /// and configured with a click event handler.
        /// </remarks>
        /// <param name="availableGroups">The list of <seealso cref="SceneObjectGroups.SceneObjectGroup"/> to be displayed in the list.</param>
        /// <param name="listItem">The VisualTreeAsset used as a template for each list item.</param>
        private void PopulateList(List<SceneObjectGroups.SceneObjectGroup> availableGroups, VisualTreeAsset listItem)
        {
            foreach (var group in availableGroups)
            {
                VisualElement item = listItem.CloneTree();
                item.Q<Label>("Label").text = group.Label;

                // Set the style for hovering depending on the editor skin
                if (EditorGUIUtility.isProSkin)
                    item.AddToClassList("listItem-dark");
                else
                    item.AddToClassList("listItem-light");

                item.userData = group;
                item.AddManipulator(new Clickable(() => OnLabelClick(item)));
                scrollView.Add(item);
            }
        }

        private void OnLabelClick(VisualElement clickedGroup)
        {
            if (clickedGroup.userData is SceneObjectGroups.SceneObjectGroup group)
            {
                onItemSelected?.Invoke(group);
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

            foreach (VisualElement child in scrollView.Children())
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
