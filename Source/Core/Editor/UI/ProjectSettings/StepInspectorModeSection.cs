using System;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Project Settings entry that lets the user switch between the legacy IMGUI
    /// Step Inspector and the new UI Toolkit panels.
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
        }
    }
}
