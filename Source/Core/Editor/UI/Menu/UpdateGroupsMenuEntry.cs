using UnityEditor;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.BuilderMenu
{
    internal static class UpdateGroupsMenuEntry
    {
        [MenuItem("Tools/VR Builder/Developer/Update Object Groups", false, 75)]
        private static void UpdateObjectGroups()
        {
            if (EditorUtility.DisplayDialog("Update Object Groups", "If this project contains any legacy tags, these will be added to the list of object groups.\n" +
                "Proceed?", "Yes", "No"))
            {
#pragma warning disable CS0618 // Type or member is obsolete
                foreach (SceneObjectTags.Tag tag in SceneObjectTags.Instance.Tags)
                {
                    if (SceneObjectGroups.Instance.GroupExists(tag.Guid) == false)
                    {
                        SceneObjectGroups.Instance.CreateGroup(tag.Label, tag.Guid);
                    }
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
