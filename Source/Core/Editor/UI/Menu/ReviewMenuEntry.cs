using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class ReviewMenuEntry
    {
        /// <summary>
        /// Redirects to the VR Builder asset store review page.
        /// </summary>
        [MenuItem("Tools/VR Builder/Leave a Review", false, 200)]
        private static void OpenReviewPage()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/game-toolkits/vr-builder-pro-toolkit-for-vr-creation-301706#reviews");
        }
    }
}
