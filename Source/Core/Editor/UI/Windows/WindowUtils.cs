using UnityEditor;
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

        internal static bool CloseStepWindow()
        {
            System.Type windowType = typeof(StepWindow);

            if (EditorWindow.HasOpenInstances<StepWindow>())
            {
                StepWindow window = EditorWindow.GetWindow<StepWindow>();
                window.Close();
                return true;
            }
            return false;
        }
    }
}
