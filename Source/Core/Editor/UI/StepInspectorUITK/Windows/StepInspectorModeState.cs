using UnityEditor;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Session-scoped flag that selects which Step Inspector strategy
    /// <see cref="GlobalEditorHandler"/> installs. Backed by Unity's
    /// <see cref="SessionState"/>, so the value survives domain reloads from
    /// script recompiles but is cleared when Unity exits — every Unity launch
    /// boots into the legacy IMGUI inspector.
    /// </summary>
    internal static class StepInspectorModeState
    {
        private const string Key = "VRBuilder.StepInspector.UseUITK";

        public static bool UseUITK
        {
            get => SessionState.GetBool(Key, false);
            set => SessionState.SetBool(Key, value);
        }
    }
}
