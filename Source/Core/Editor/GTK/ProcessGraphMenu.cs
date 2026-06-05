using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.ProcessAssets;

namespace MindPort.GTKProcessEditor.Editor
{
    // Menu entry that draws the selected VR Builder process (chapter 1) into a Graph Toolkit window.
    public static class ProcessGraphMenu
    {
        [MenuItem("Tools/VR Builder/GTK Process Editor...", false, 16)]
        private static void Open()
        {
            string processName = ResolveProcessName();
            if (string.IsNullOrEmpty(processName))
            {
                EditorUtility.DisplayDialog("GTK Process Editor",
                    "No VR Builder process found. Create or select one in the classic Process Editor first.",
                    "OK");
                return;
            }

            // Load the live process shared with the classic editor and the Step Inspector.
            GlobalEditorHandler.SetCurrentProcess(processName);
            IProcess process = GlobalEditorHandler.GetCurrentProcess();
            if (process == null)
            {
                Debug.LogError($"[GTK Process Editor] Could not load process '{processName}'.");
                return;
            }

            // Set the chapter too, so step edits don't NPE anything reading GetCurrentChapter().
            GlobalEditorHandler.SetCurrentChapter(process.Data.FirstChapter);

            string assetPath = ProcessGraphBuilder.Build(processName, process);
            if (assetPath != null)
            {
                OpenWindow(assetPath);
            }
        }

        private static string ResolveProcessName()
        {
            if (RuntimeConfigurator.Exists)
            {
                string selectedPath = RuntimeConfigurator.Instance.GetSelectedProcess();
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    return ProcessAssetUtils.GetProcessNameFromPath(selectedPath);
                }
            }

            return ProcessAssetUtils.GetAllProcesses().FirstOrDefault();
        }

        private static void OpenWindow(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
            }
        }
    }
}
