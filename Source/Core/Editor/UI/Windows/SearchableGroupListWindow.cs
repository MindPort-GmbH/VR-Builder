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
    public class SearchableGroupListWindow : EditorWindow
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
        /// The ScrollView containing the list of groups.
        /// </summary>
        private ScrollView scrollView;

        /// <summary>
        /// The list of groups which will be used.
        /// </summary>
        private List<SceneObjectGroups.SceneObjectGroup> availableGroups;

        /// <summary>
        /// The list of groups which will be used.
        /// </summary>
        private List<SceneObjectGroups.SceneObjectGroup> selectedGroups;

        /// <summary>
        /// Callback to invoke when a group is selected.
        /// </summary>
        private Action<List<SceneObjectGroups.SceneObjectGroup>> onItemSelected;

        public void SetItemsSelectedCallBack(Action<List<SceneObjectGroups.SceneObjectGroup>> onItemSelected)
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
            selectedGroups = new List<SceneObjectGroups.SceneObjectGroup>();
        }

        public void CreateGUI()
        {
            // Initialize UI from UXML
            selectItemsPanel.CloneTree(rootVisualElement);

            //Set the style of the window depending on the editor skin
            rootVisualElement.AddToClassList("searchableList");
            SetSkinDependingOnUnitySkin(rootVisualElement, "popupStyle-dark", "popupStyle-light");

            // Connect all UI elements
            Button assignsSelectedGroupsButton = rootVisualElement.Q<Button>("AssignsSelectedGroupsButton");
            assignsSelectedGroupsButton.clicked += OnAssignGroups;

            Button deselectAllGroupsButton = rootVisualElement.Q<Button>("DeselectAllGroupsButton");
            deselectAllGroupsButton.clicked += DeselectAll;

            ObjectField selectAllGroupsObjectField = rootVisualElement.Q<ObjectField>("SelectAllGroupsObjectField");
            selectAllGroupsObjectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is ProcessSceneObject sco)
                {
                    IEnumerable<Guid> groups = sco.Guids; // Your list of Guids
                    IEnumerable<VisualElement> groupElements = scrollView.Children(); // Your collection of VisualElements

                    List<VisualElement> groupsToSelect = groupElements
                        .Where(element => element.userData is SceneObjectGroups.SceneObjectGroup group && groups.Contains(group.Guid))
                        .ToList();

                    foreach (VisualElement groupElement in groupsToSelect)
                    {
                        SelectGroup(groupElement, additiveSelect: true);
                    }

                    // Clear the object after the next frame
                    EditorApplication.delayCall += () => { selectAllGroupsObjectField.value = null; };
                }
            });

            ToolbarSearchField searchField = rootVisualElement.Q<ToolbarSearchField>("SearchGroupField");
            scrollView = rootVisualElement.Q<ScrollView>("GroupList");

            // Populate the list
            PopulateList(availableGroups, listItem);

            //Add event listener to the search field
            searchField.RegisterValueChangedCallback(evt => FilterListByPartialMatch(evt.newValue));
            //Focus the search field after it is ready
            EditorApplication.delayCall += () => { searchField.Focus(); };
        }

        private void OnAssignGroups()
        {
            Close();
            onItemSelected.Invoke(selectedGroups);
        }

        /// <summary>
        /// Set the groups to be shown.
        /// </summary>
        /// <param name="availableGroups">The list of groups.</param>
        public void SetAvailableGroups(List<SceneObjectGroups.SceneObjectGroup> availableGroups)
        {
            this.availableGroups = availableGroups.OrderBy(t => t.Label).ToList();
        }

        /// <summary>
        /// Update the groups to be displayed in the list.
        /// </summary>
        /// <param name="availableGroups"></param> 
        public void UpdateAvailableGroups(List<SceneObjectGroups.SceneObjectGroup> availableGroups)
        {
            SetAvailableGroups(availableGroups);
            scrollView.Clear();
            PopulateList(this.availableGroups, listItem);
        }

        /// <summary>
        /// Select the given groups in the list.
        /// </summary>
        public void PreSelectGroups(List<SceneObjectGroups.SceneObjectGroup> groupsToSelect)
        {
            foreach (VisualElement child in scrollView.Children())
            {
                if (child.userData is SceneObjectGroups.SceneObjectGroup group && groupsToSelect.Contains(group))
                {
                    SelectGroup(child, additiveSelect: true);
                }
            }
        }

        /// <summary>
        /// Populates the ScrollView with a list of <see cref="SceneObjectGroups.SceneObjectGroup"/>.
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
            if (availableGroups == null)
                availableGroups = new List<SceneObjectGroups.SceneObjectGroup>(SceneObjectGroups.Instance.Groups);

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
                item.AddManipulator(new Clickable(() => SelectGroup(item)));
                scrollView.Add(item);
            }
        }

        /// <summary>
        /// Selects or deselects a <see cref="SceneObjectGroups.SceneObjectGroup"/> based on the provided VisualElement.
        /// </summary>
        /// <param name="selectedGroup">The VisualElement representing the <see cref="SceneObjectGroups.SceneObjectGroup"/> to be selected or deselected.</param>
        /// <param name="additiveSelect">A boolean indicating whether to deselect the <see cref="SceneObjectGroups.SceneObjectGroup"/> if is already selected.</param>
        private void SelectGroup(VisualElement selectedGroup, bool additiveSelect = false)
        {
            if (selectedGroup.userData is SceneObjectGroups.SceneObjectGroup group)
            {
                if (selectedGroups.Contains(group))
                {
                    if (!additiveSelect)
                    {
                        selectedGroups.Remove(group);
                        RemoveSkinDependingOnUnitySkin(selectedGroup, "listItem-selected-dark", "listItem-selected-light");
                    }
                }
                else
                {
                    selectedGroups.Add(group);
                    SetSkinDependingOnUnitySkin(selectedGroup, "listItem-selected-dark", "listItem-selected-light");
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

        /// <summary>
        /// Deselects all the <see cref="SceneObjectGroups.SceneObjectGroup"/> in the list.
        /// </summary>
        private void DeselectAll()
        {
            foreach (VisualElement selectedGroup in scrollView.Children())
            {
                if (selectedGroup.userData is SceneObjectGroups.SceneObjectGroup group)
                {
                    selectedGroups.Remove(group);
                    RemoveSkinDependingOnUnitySkin(selectedGroup, "listItem-selected-dark", "listItem-selected-light");
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