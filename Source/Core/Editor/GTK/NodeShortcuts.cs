using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Editor;

namespace MindPort.GTKProcessEditor.Editor
{
    // Shift+R refreshes the focused GTK process graph. Rebindable under Edit ▸ Shortcuts.
    public static class NodeShortcuts
    {
        private static double lastActionTime;

        [ClutchShortcut("VR Builder/GTK Process Editor/Refresh Graph", KeyCode.R, ShortcutModifiers.Shift)]
        private static void RefreshGraph(ShortcutArguments args)
        {
            if (args.stage != ShortcutStage.Begin || !Allow())
            {
                return;
            }

            ProcessGraph graph = GtkGraphAccess.GetFocusedProcessGraph();
            if (graph == null || string.IsNullOrEmpty(graph.ProcessName))
            {
                return;
            }

            IProcess process = GlobalEditorHandler.GetCurrentProcess();
            if (process != null && process.Data.Name == graph.ProcessName)
            {
                ProcessGraphBuilder.RefreshOpenGraph(graph.ProcessName, process);
            }
        }

        // Debounce so a single press cannot trigger more than one action.
        private static bool Allow()
        {
            if (EditorApplication.timeSinceStartup - lastActionTime < 0.25)
            {
                return false;
            }

            lastActionTime = EditorApplication.timeSinceStartup;
            return true;
        }
    }
}
