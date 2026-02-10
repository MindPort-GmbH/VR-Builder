using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class RoadmapMenuEntry
    {
        /// <summary>
        /// Redirects to the VR Builder asset store review page.
        /// </summary>
        [MenuItem("Tools/VR Builder/Help/Roadmap", false, 130)]
        private static void OpenRoadmapPage()
        {
            Application.OpenURL("https://www.mindport.co/vr-builder/roadmap?utm_source=unity_editor&utm_medium=referral&utm_campaign=from_unity&utm_id=from_unity");
        }
    }
}
