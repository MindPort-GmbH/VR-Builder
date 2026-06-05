using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.UI.Windows;

namespace MindPort.GTKProcessEditor.Editor
{
    // Opens the VR Builder Step Inspector for the node selected in the GTK window (polled; GTK has no
    // selection-changed event). Guarded so a package change degrades to "no inspector sync" rather than errors.
    [InitializeOnLoad]
    public static class StepSelectionProbe
    {
        private const double PollInterval = 0.15;

        private static double lastPollTime;
        private static string lastPushedGuid = string.Empty;
        private static bool loggedError;

        static StepSelectionProbe()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup - lastPollTime < PollInterval)
            {
                return;
            }

            lastPollTime = EditorApplication.timeSinceStartup;

            try
            {
                Poll();
            }
            catch (Exception exception)
            {
                if (!loggedError)
                {
                    loggedError = true;
                    Debug.LogWarning($"[GTK Process Editor] Step Inspector selection sync error (will keep retrying): {exception.Message}");
                }
            }
        }

        private static void Poll()
        {
            StepNode selected = null;
            foreach (INode node in GtkGraphAccess.GetSelectedNodes())
            {
                if (node is StepNode stepNode)
                {
                    selected = stepNode;
                    break;
                }
            }

            PushToInspector(selected);
        }

        private static void PushToInspector(StepNode selected)
        {
            if (selected == null || string.IsNullOrEmpty(selected.StepGuid))
            {
                lastPushedGuid = string.Empty;
                return;
            }

            if (selected.StepGuid == lastPushedGuid)
            {
                return;
            }

            IStep step = FindStep(GlobalEditorHandler.GetCurrentProcess(), selected.StepGuid);
            if (step == null)
            {
                return;
            }

            lastPushedGuid = selected.StepGuid;

            // Show the step without stealing focus from the graph.
            StepWindow inspector = StepWindow.GetInstance(false);
            inspector.SetStep(step);
            inspector.Repaint();
        }

        private static IStep FindStep(IProcess process, string stepGuid)
        {
            if (process == null || string.IsNullOrEmpty(stepGuid))
            {
                return null;
            }

            foreach (IChapter chapter in process.Data.Chapters)
            {
                foreach (IStep step in chapter.Data.Steps)
                {
                    if (step.StepMetadata.Guid.ToString() == stepGuid)
                    {
                        return step;
                    }
                }
            }

            return null;
        }
    }
}
