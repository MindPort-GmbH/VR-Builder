using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.BuilderMenu
{
    internal static class ReviewMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to the MindPort community.
        /// </summary>
        [MenuItem("Tools/VR Builder/Leave a Review", false, 128)]
        private static void OpenCommunityPage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/visual-scripting/vr-builder-open-source-toolkit-for-vr-creation-201913#reviews");
        }
    }
}
