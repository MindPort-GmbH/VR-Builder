using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;

namespace VRBuilder.Core.Editor.UI.Windows
{
    public class WindowUtils
    {
        internal static bool CloseProcessEditorWindow()
        {
            System.Type windowType = typeof(ProcessEditorWindow);

            if (EditorWindow.HasOpenInstances<ProcessEditorWindow>())
            {
                ProcessEditorWindow window = EditorWindow.GetWindow<ProcessEditorWindow>();
                window.Close();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Closes every open Step Inspector window, both the legacy <see cref="StepWindow"/> and the
        /// UITK panels. Returns true if at least one window was closed.
        /// </summary>
        /// <remarks>
        /// Keyed on <see cref="IStepView"/> — the same contract <see cref="GlobalEditorHandler"/>
        /// uses to identify a step window — so any editor window hosting a step inspector is covered.
        /// </remarks>
        internal static bool CloseStepWindow()
        {
            bool closedAny = false;

            // Iterate a copy: Close() destroys the window, which mutates the live window list.
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window is IStepView)
                {
                    window.Close();
                    closedAny = true;
                }
            }

            return closedAny;
        }

        /// <summary>
        /// True if any Step Inspector window (legacy <see cref="StepWindow"/> or a UITK panel) is open.
        /// </summary>
        internal static bool IsAnyStepWindowOpen()
        {
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window is IStepView)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
