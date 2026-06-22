using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Views;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Opens the legacy searchable Scene Object Group picker (<see cref="SearchableGroupListPopup"/>)
    /// anchored under a UITK button. Used instead of a flat <c>GenericMenu</c> so the list stays
    /// usable when many groups exist.
    /// </summary>
    internal static class GroupPickerPopup
    {
        public static void Show(
            VisualElement activator,
            IEnumerable<SceneObjectGroups.SceneObjectGroup> availableGroups,
            Action<SceneObjectGroups.SceneObjectGroup> onSelected,
            bool firstItemIsProcessSceneObject = false)
        {
            VisualTreeAsset searchableList = ViewDictionary.LoadAsset(ViewDictionary.EnumType.SearchableList);
            VisualTreeAsset groupListItem = ViewDictionary.LoadAsset(ViewDictionary.EnumType.GroupListItem);

            SearchableGroupListPopup content = new SearchableGroupListPopup(onSelected, searchableList, groupListItem);
            content.SetAvailableGroups(availableGroups, firstItemIsProcessSceneObject);
            content.SetWindowSize(windowWith: activator.resolvedStyle.width);

            UnityEditor.PopupWindow.Show(activator.worldBound, content);
        }
    }
}
