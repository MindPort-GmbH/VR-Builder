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

        public void SetItemsSelectedCallBack(Action<List<SceneObjectTags.Tag>> onItemSelected)
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
            SetSkinDependingOnUnitySkin(rootVisualElement, "searchableList-dark", "searchableList-light");

            // Connect all UI elements
            Button assignsSelectedTagsButton = rootVisualElement.Q<Button>("AssignsSelectedTagsButton");
            assignsSelectedTagsButton.clicked += OnAssignTags;

            Button deselectAllTagsButton = rootVisualElement.Q<Button>("DeselectAllTagsButton");
            deselectAllTagsButton.clicked += DeselectAll;

            ObjectField selectAllTagsObjectField = rootVisualElement.Q<ObjectField>("SelectAllTagsObjectField");
            selectAllTagsObjectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is ProcessSceneObject sco)
                {
                    IEnumerable<Guid> tags = sco.Guids; // Your list of Guids
                    IEnumerable<VisualElement> tagElements = tagScrollView.Children(); // Your collection of VisualElements

                    List<VisualElement> tagsToSelect = tagElements
                        .Where(element => element.userData is SceneObjectTags.Tag tag && tags.Contains(tag.Guid))
                        .ToList();

                    foreach (VisualElement tagElement in tagsToSelect)
                    {
                        SelectTag(tagElement, additiveSelect: true);
                    }

                    // Clear the object after the next frame
                    EditorApplication.delayCall += () => { selectAllTagsObjectField.value = null; };
                }
            });

            ToolbarSearchField searchField = rootVisualElement.Q<ToolbarSearchField>("SearchTagField");
            tagScrollView = rootVisualElement.Q<ScrollView>("TagList");

            // Populate the list
            PopulateList(availableTags, listItem);

            //Add event listener to the search field
            searchField.RegisterValueChangedCallback(evt => FilterListByPartialMatch(evt.newValue));
            //Focus the search field after it is ready
            EditorApplication.delayCall += () => { searchField.Focus(); };
        }

        private void OnAssignTags()
        {
            Close();
            onItemSelected.Invoke(selectedTags);
        }

        /// <summary>
        /// Set the tags to be shown.
        /// </summary>
        /// <param name="availableTags">The list of tags.</param>
        public void SetAvailableTags(List<SceneObjectTags.Tag> availableTags)
        {
            this.availableTags = availableTags.OrderBy(t => t.Label).ToList();
        }

        /// <summary>
        /// Update the tags to be displayed in the list.
        /// </summary>
        /// <param name="availableTags"></param> 
        public void UpdateAvailableTags(List<SceneObjectTags.Tag> availableTags)
        {
            SetAvailableTags(availableTags);
            tagScrollView.Clear();
            PopulateList(this.availableTags, listItem);
        }

        /// <summary>
        /// Select the given tags in the list.
        /// </summary>
        public void PreSelectTags(List<SceneObjectTags.Tag> tagsToSelect)
        {
            foreach (VisualElement child in tagScrollView.Children())
            {
                if (child.userData is SceneObjectTags.Tag tag && tagsToSelect.Contains(tag))
                {
                    SelectTag(child, additiveSelect: true);
                }
            }
        }

        /// <summary>
        /// Populates the ScrollView with a list of <see cref="SceneObjectTags.Tag"/>.
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
            if (availableTags == null)
                availableTags = new List<SceneObjectTags.Tag>(SceneObjectTags.Instance.Tags);

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

        /// <summary>
        /// Selects or deselects a <see cref="SceneObjectTags.Tag"/> based on the provided VisualElement.
        /// </summary>
        /// <param name="selectedTag">The VisualElement representing the <see cref="SceneObjectTags.Tag"/> to be selected or deselected.</param>
        /// <param name="additiveSelect">A boolean indicating whether to deselect the <see cref="SceneObjectTags.Tag"/> if is already selected.</param>
        private void SelectTag(VisualElement selectedTag, bool additiveSelect = false)
        {
            if (selectedTag.userData is SceneObjectTags.Tag tag)
            {
                if (selectedTags.Contains(tag))
                {
                    if (!additiveSelect)
                    {
                        selectedTags.Remove(tag);
                        RemoveSkinDependingOnUnitySkin(selectedTag, "listItem-selected-dark", "listItem-selected-light");
                    }
                }
                else
                {
                    selectedTags.Add(tag);
                    SetSkinDependingOnUnitySkin(selectedTag, "listItem-selected-dark", "listItem-selected-light");
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

        /// <summary>
        /// Deselects all the <see cref="SceneObjectTags.Tag"/> in the list.
        /// </summary>
        private void DeselectAll()
        {
            foreach (VisualElement selectedTag in tagScrollView.Children())
            {
                if (selectedTag.userData is SceneObjectTags.Tag tag)
                {
                    selectedTags.Remove(tag);
                    RemoveSkinDependingOnUnitySkin(rootVisualElement, "searchableList-dark", "searchableList-light");
                }
            }
        }


        /// <summary>
        /// Sets the skin of a VisualElement depending on the current Unity skin.
        /// </summary>
        /// <param name="visualElement">The VisualElement to set the skin for.</param>
        /// <param name="proSkinClass">The CSS class to add if the Unity skin is Pro skin.</param>
        /// <param name="defaultSkinClass">The CSS class to add if the Unity skin is not Pro skin.</param>
        private void SetSkinDependingOnUnitySkin(VisualElement visualElement, string proSkinClass, string defaultSkinClass)
        {
            if (EditorGUIUtility.isProSkin)
            {
                visualElement.AddToClassList(proSkinClass);
            }
            else
            {
                visualElement.AddToClassList(defaultSkinClass);
            }
        }

        /// <summary>
        /// Removes a skin class from a VisualElement depending on the current Unity skin.
        /// </summary>
        /// <param name="visualElement">The VisualElement to remove the skin class from.</param>
        /// <param name="proSkinClass">The skin class to remove when using the Pro skin.</param>
        /// <param name="defaultSkinClass">The skin class to remove when using the Default skin.</param>
        private void RemoveSkinDependingOnUnitySkin(VisualElement visualElement, string proSkinClass, string defaultSkinClass)
        {
            if (EditorGUIUtility.isProSkin)
            {
                visualElement.RemoveFromClassList(proSkinClass);
            }
            else
            {
                visualElement.RemoveFromClassList(defaultSkinClass);
            }
        }
    }
}