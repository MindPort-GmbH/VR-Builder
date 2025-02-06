using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.Views
{
    /// <summary>
    /// Helper to fill a GroupListItem.uxml UI item.
    /// </summary>
    public class GroupListItem
    {
        /// <summary>
        /// String to display when the group count is not available.
        /// </summary>
        public const string GroupCountNotAvailableText = "n/a";

        /// <summary>
        /// Fills a GroupListItem.xml View with the data from <seealso cref="SceneObjectGroups.SceneObjectGroup"/> data.
        /// </summary>
        /// <param name="groupListItem">The VisualElement representing the group list item.</param>
        /// <param name="name">The name of the Group or GameObject containing Process Scene Object.</param>
        /// <param name="isPreviewInContext">A flag indicating whether <see cref="GroupCountNotAvailableText"/> should be display instead of <see cref="groupCount"/> .</param>
        /// <param name="referencedSceneObjects">Collection of scene objects to check group assignment</param>
        /// <param name="elementIsUniqueIdDisplayName">A flag indicating whether the element represents a unique <see cref="ProcessSceneObject"/> and not a <seealso cref="SceneObjectGroups.SceneObjectGroup"/>. Default value is false.</param>
        public static void FillGroupListItem(VisualElement groupListItem, string name, bool isPreviewInContext = false, IEnumerable<ISceneObject> referencedSceneObjects = null, bool elementIsUniqueIdDisplayName = false)
        {
            Label groupReferenceCountLabel = groupListItem.Q<Label>("RefCount");
            VisualElement groupListElement = groupListItem.Q<VisualElement>("GroupListElement");
            Label groupLabel = groupListItem.Q<Label>("Name");

            groupLabel.text = name;

            if (elementIsUniqueIdDisplayName)
            {
                groupLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                groupListElement.tooltip = $"Reference to GameObject: {name}";
                groupReferenceCountLabel.text = GroupCountNotAvailableText;
            }
            else
            {
                //check if the enumerable is null
                if (referencedSceneObjects != null)
                {
                    //fixed possible multiple enumeration
                    var sceneObjects = referencedSceneObjects as ISceneObject[] ?? referencedSceneObjects.ToArray();
                    int count = sceneObjects.Length;
                    groupReferenceCountLabel.text = count.ToString();
                    if (count == 0)
                    {
                        groupListElement.tooltip = "Group contains no objects.";
                    }
                    else
                    {
                        groupListElement.tooltip = $"Group is assigned to: {sceneObjects.Aggregate("", (acc, sceneObject) => acc + "\n- " + sceneObject.GameObject.name)}";
                    }
                }
                if (isPreviewInContext)
                {
                    groupListElement.tooltip += "\nNote: Object count might be inaccurate if object is outside of scene context.";
                }
            }
        }
    }
}
