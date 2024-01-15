using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI.Windows
{
    public class SearchableTagListWindow : EditorWindow
    {
        /// <summary>
        /// Root VisualTreeAsset for the searchable list.
        /// </summary>
        [SerializeField]
        private VisualTreeAsset selectItemsPanel = default;

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
        private List<SceneObjectTags.Tag> availableTags;

        /// <summary>
        /// The list of tags which will be used.
        /// </summary>
        private List<SceneObjectTags.Tag> selectedTags;

        /// <summary>
        /// Callback to invoke when a tag is selected.
        /// </summary>
        private Action<List<SceneObjectTags.Tag>> onItemSelected;

        public void Initialize(Action<List<SceneObjectTags.Tag>> onItemSelected)
        {
            this.onItemSelected = onItemSelected;
            windowSize = minWindowSize;
        }

        public Vector2 GetWindowSize()
        {
            return windowSize;
        }

        public void OnGUI()
        {
            // intentionally left blank
        }

        private void Awake()
        {
            selectedTags = new List<SceneObjectTags.Tag>();
        }

        public void CreateGUI()
        {
            // Initialize UI from UXML
            selectItemsPanel.CloneTree(rootVisualElement);

            //Set the style of the window depending on the editor skin
            rootVisualElement.AddToClassList("searchableList");
            if (EditorGUIUtility.isProSkin)
                rootVisualElement.AddToClassList("searchableList-dark");
            else
                rootVisualElement.AddToClassList("searchableList-light");

            // Connect all UI elements
            Button assignsSelectedTagsButton = rootVisualElement.Q<Button>("AssignsSelectedTagsButton");
            assignsSelectedTagsButton.clicked += OnTagsSelected;

            Button deselectAllTagsButton = rootVisualElement.Q<Button>("DeselectAllTagsButton");
            deselectAllTagsButton.clicked += DeselectAll;

            ObjectField selectAllTagsObjectField = rootVisualElement.Q<ObjectField>("SelectAllTagsObjectField");
            selectAllTagsObjectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is ProcessSceneObject sco)
                {
                    IEnumerable<Guid> tags = sco.Tags; // Your list of Guids
                    IEnumerable<VisualElement> tagElements = tagScrollView.Children(); // Your collection of VisualElements

                    List<VisualElement> tagsToSelect = tagElements
                        .Where(element => element.userData is SceneObjectTags.Tag tag && tags.Contains(tag.Guid))
                        .ToList();

                    foreach (VisualElement tagElement in tagsToSelect)
                    {
                        SelectTag(tagElement, removeIfSelected: false);
                    }

                    // Clear the object after the next frame
                    EditorApplication.delayCall += () => { selectAllTagsObjectField.value = null; };
                }
            });

            ToolbarSearchField searchField = rootVisualElement.Q<ToolbarSearchField>("SearchTagField");
            tagScrollView = rootVisualElement.Q<ScrollView>("TagList");

            // Populate the list
            if (availableTags == null)
                availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);
            PopulateList(availableTags, listItem);

            //Add event listener to the search field
            searchField.RegisterValueChangedCallback(evt => FilterListByPartialMatch(evt.newValue));
            //Focus the search field after it is ready
            EditorApplication.delayCall += () => { searchField.Focus(); };
        }

        private void DeselectAll()
        {
            foreach (VisualElement selectedTag in tagScrollView.Children())
            {
                if (selectedTag.userData is SceneObjectTags.Tag tag)
                {
                    selectedTags.Remove(tag);
                    if (EditorGUIUtility.isProSkin)
                    {
                        selectedTag.RemoveFromClassList("listItem-selected-dark");
                    }
                    else
                    {
                        selectedTag.RemoveFromClassList("listItem-selected-light");
                    }
                }
            }
        }

        private void OnTagsSelected()
        {
            onItemSelected.Invoke(selectedTags);
            Close();
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
            this.availableTags = availableTags.OrderBy(t => t.Label).ToList();
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
                item.AddManipulator(new Clickable(() => SelectTag(item)));
                tagScrollView.Add(item);
            }
        }

        private void SelectTag(VisualElement selectedTag, bool removeIfSelected = true)
        {
            if (selectedTag.userData is SceneObjectTags.Tag tag)
            {
                if (selectedTags.Contains(tag))
                {
                    if (removeIfSelected)
                    {
                        selectedTags.Remove(tag);
                        if (EditorGUIUtility.isProSkin)
                        {
                            selectedTag.RemoveFromClassList("listItem-selected-dark");
                        }
                        else
                        {
                            selectedTag.RemoveFromClassList("listItem-selected-light");
                        }
                    }
                }
                else
                {
                    selectedTags.Add(tag);
                    if (EditorGUIUtility.isProSkin)
                    {
                        selectedTag.AddToClassList("listItem-selected-dark");
                    }
                    else
                    {
                        selectedTag.AddToClassList("listItem-selected-light");
                    }
                }
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