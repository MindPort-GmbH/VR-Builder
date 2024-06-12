using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

public class GroupListItem
{
    public const string GroupCountNotAvailableText = "n/a";

    /// <summary>
    /// Fills a GroupListItem.xml View with the data from <seealso cref="SceneObjectGroups.SceneObjectGroup"> data.
    /// </summary>
    /// <param name="groupListElement">The VisualElement representing the group list item.</param>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="isPreviewInContext">A flag indicating whether <see cref="GroupCountNotAvailableText"/> should be display instead of <see cref="groupCount"> .</param>
    /// <param name="groupReferenceCount">The count of <seealso cref="ProcessSceneObject"/>s in the group. Default value is -1.</param>
    /// <param name="elementIsUniqueIdDisplayName">A flag indicating whether the element represents a unique <see cref="ProcessSceneObject"/> and not a <seealso cref="SceneObjectGroups.SceneObjectGroup">. Default value is false.</param>
    public static void FillGroupListItem(VisualElement groupListElement, string groupName, bool isPreviewInContext = false, int groupReferenceCount = -1, bool elementIsUniqueIdDisplayName = false)
    {
        groupListElement.Q<Label>("RefCount").text = isPreviewInContext ? GroupCountNotAvailableText : groupReferenceCount.ToString();

        Label groupLabel = groupListElement.Q<Label>("Name");
        groupLabel.text = groupName;
        if (elementIsUniqueIdDisplayName)
        {
            groupLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            groupLabel.tooltip = "GameObject Name";
        }
    }
}