using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.Menu
{
    internal static class AddonsMenuEntry
    {
        /// <summary>
        /// Allows to open the URL to webinar.
        /// </summary>
        [MenuItem("Tools/VR Builder/Help/Add-ons and Integrations", false, 131)]
        private static void OpenAddonsPage()
        {
            Application.OpenURL("https://www.mindport.co/vr-builder-add-ons-and-integrations?utm_source=unity_editor&utm_medium=referral&utm_campaign=tools_addons");
        }
    }
}
