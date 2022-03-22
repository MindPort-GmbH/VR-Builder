using UnityEngine;
using System;
using UnityEditor;

namespace VRBuilder.Editor.UI
{
    internal class InteractionComponentSettingsSection : IProjectSettingsSection
    {
        public string Title { get; } = "Interaction Component Configuration";
        public Type TargetPageProvider { get; } = typeof(BuilderSettingsProvider);
        public int Priority { get; } = 500;

        public void OnGUI(string searchContext)
        {
            InteractionComponentSettings config = InteractionComponentSettings.Instance;

            EditorGUI.BeginChangeCheck();

            config.EnableXRInteractionComponent = GUILayout.Toggle(config.EnableXRInteractionComponent, "Enable XR Interaction Component", BuilderEditorStyles.Toggle);

            if (EditorGUI.EndChangeCheck())
            {
                InteractionComponentSettings.Instance.Save();
            }
        }
    }
}
