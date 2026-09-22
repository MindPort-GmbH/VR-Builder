using UnityEditor;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Persistent flag that selects which Step Inspector strategy
    /// <see cref="GlobalEditorHandler"/> installs. Backed by Unity's
    /// <see cref="EditorPrefs"/>, so the selection survives editor restarts
    /// and defaults to the legacy IMGUI inspector.
    /// </summary>
    internal static class StepInspectorModeState
    {
        private const string Key = "VRBuilder.StepInspector.UseUITK";

        public static bool UseUITK
        {
            get => EditorPrefs.GetBool(Key, false);
            set => EditorPrefs.SetBool(Key, value);
        }
    }
}
