using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class AddonsMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to webinar.
        /// </summary>
        [MenuItem("Tools/VR Builder/Add-ons and Integrations", false, 129)]
        private static void OpenAddonsPage()
        {
            Application.OpenURL("https://www.mindport.co/vr-builder-add-ons-and-integrations?utm_source=unityeditor&utm_medium=unityeditor&utm_campaign=from_unity&utm_id=from_unity");
        }
    }
}
