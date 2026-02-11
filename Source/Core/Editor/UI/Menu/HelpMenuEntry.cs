using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class HelpMenuEntry
    {
        /// <summary>
        /// Redirects to the VR Builder asset store review page.
        /// </summary>
        [MenuItem("Tools/VR Builder/Help/Contact us via Email", false, 500)]
        private static void OpenReviewPage()
        {
            Application.OpenURL("mailto:support@mindport.co");
        }
    }
}
