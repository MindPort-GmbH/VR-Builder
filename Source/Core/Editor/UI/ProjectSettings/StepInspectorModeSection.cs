using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Project Settings entry that lets the user switch between the legacy IMGUI
    /// Step Inspector and the new UI Toolkit panels. The selection is session-only
    /// (see <see cref="StepInspectorModeState"/>) — closing Unity always resets to
    /// the legacy inspector.
    /// </summary>
    public class StepInspectorModeSection : IProjectSettingsSection
    {
        public string Title => "Step Inspector";

        public Type TargetPageProvider => typeof(BuilderPageProvider);

        public int Priority => 900;

        public void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();

            bool useUITK = GUILayout.Toggle(
                StepInspectorModeState.UseUITK,
                "Use UI Toolkit Step Inspector (experimental)",
                BuilderEditorStyles.Toggle);

            if (EditorGUI.EndChangeCheck())
            {
                StepInspectorModeState.UseUITK = useUITK;
                GlobalEditorHandler.ApplyStepInspectorMode();
            }

            EditorGUILayout.HelpBox(
                "Session-only setting. Unity always boots into the legacy Step Inspector; " +
                "this toggle switches the active inspector for the rest of the editor session.",
                MessageType.Info);
        }
    }
}
