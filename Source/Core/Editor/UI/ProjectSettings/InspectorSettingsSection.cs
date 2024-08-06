using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    public class InspectorSettingsSection : IProjectSettingsSection
    {
        public string Title => "Inspector";

        public Type TargetPageProvider => typeof(AdvancedSettingsProvider);

        public int Priority => 1000;

        public void OnGUI(string searchContext)
        {
            AdvancedSettings config = AdvancedSettings.Instance;

            EditorGUI.BeginChangeCheck();

            config.ShowExpertInfo = GUILayout.Toggle(config.ShowExpertInfo, "Show advanced information in the inspector of VR Builder components", BuilderEditorStyles.Toggle);

            config.ShowSceneObjectCreationDialog = GUILayout.Toggle(config.ShowSceneObjectCreationDialog, "Show dialog before creating scene objects", BuilderEditorStyles.Toggle);

            config.AutoAddProperties = GUILayout.Toggle(config.AutoAddProperties, "Automatically add required properties", BuilderEditorStyles.Toggle);

            if (EditorGUI.EndChangeCheck())
            {
                config.Save();
            }
        }
    }
}
