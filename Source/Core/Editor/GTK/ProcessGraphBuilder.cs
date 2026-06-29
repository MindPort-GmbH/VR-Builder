using System.IO;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;

namespace MindPort.GTKProcessEditor.Editor
{
    // Builds the GTK graph asset for a process and renders chapter 1. Used by the menu and the Shift+R refresh.
    public static class ProcessGraphBuilder
    {
        // Generated .vrbgtk view assets live in the project (regenerated from the process), not in the package.
        private const string GraphsFolder = "Assets/MindPort/GTK Process Editor/Graphs";

        public static string GetAssetPath(string processName)
        {
            return $"{GraphsFolder}/{processName}.{ProcessGraph.FileExtension}";
        }

        public static string Build(string processName, IProcess process)
        {
            if (process == null || string.IsNullOrEmpty(processName))
            {
                return null;
            }

            EnsureFolderExists();
            string assetPath = GetAssetPath(processName);

            ProcessGraph graph = GraphDatabase.CreateGraph<ProcessGraph>(assetPath);
            if (graph == null)
            {
                Debug.LogError($"[GTK Process Editor] Could not create graph asset at '{assetPath}'.");
                return null;
            }

            graph.ProcessName = processName;
            int nodeCount = ProcessGraphRenderer.Render(graph, process);

            AssetDatabase.ImportAsset(assetPath);

            Debug.Log($"[GTK Process Editor] Drew process '{processName}' (chapter 1) with {nodeCount} node(s).");
            return assetPath;
        }

        // Regenerates the asset and reloads the window (close + reopen) — the only way GTK shows external changes.
        public static void RefreshOpenGraph(string processName, IProcess process)
        {
            if (process == null || string.IsNullOrEmpty(processName))
            {
                return;
            }

            EditorWindow graphWindow = FindOpenGraphWindow(processName);

            string assetPath = Build(processName, process);
            if (assetPath == null)
            {
                return;
            }

            if (graphWindow != null)
            {
                graphWindow.Close();
            }
            else
            {
                Debug.LogWarning("[GTK Process Editor] Could not find the open graph window to reload; reopen it from the menu.");
            }

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
            }
        }

        // Shift+R is pressed inside the graph window, so the focused window is it; fall back to a name search.
        private static EditorWindow FindOpenGraphWindow(string processName)
        {
            EditorWindow focused = EditorWindow.focusedWindow;
            if (focused != null && focused.rootVisualElement != null
                && GtkGraphAccess.ContainsGraphView(focused.rootVisualElement))
            {
                return focused;
            }

            return GtkGraphAccess.FindProcessGraphWindow(processName);
        }

        private static void EnsureFolderExists()
        {
            if (!Directory.Exists(GraphsFolder))
            {
                Directory.CreateDirectory(GraphsFolder);
                AssetDatabase.Refresh();
            }
        }
    }
}
