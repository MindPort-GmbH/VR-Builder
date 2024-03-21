using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Editor.Utils
{
    public static class ProcessUpdater
    {
        [MenuItem("Tools/VR Builder/Developer/Update Process in Scene", false, 70)]
        private static void UpdateProcessMenuEntry()
        {
            if (RuntimeConfigurator.Exists == false)
            {
                Debug.LogError("This is not a VR Builder scene");
                return;
            }

            IProcess process = GlobalEditorHandler.GetCurrentProcess();

            if (process == null)
            {
                Debug.LogError("No active process found.");
                return;
            }

            UpdateProcess(process);
        }

        private static void UpdateProcess(IProcess process)
        {
            // Iterate through all entities in the process.

            // For each behavior and condition, check if there is a custom converter and if so use it to replace it with a new version.

            // If there is no custom converter, apply the default one.
        }
    }
}
