using UnityEngine;
using System;
using UnityEditor;

namespace VRBuilder.Editor.UI
{
    internal class InteractionSettingsSection : IProjectSettingsSection
    {
        public string Title { get; } = "Interaction Settings";
        public Type TargetPageProvider { get; } = typeof(BuilderSettingsProvider);
        public int Priority { get; } = 250;

        public void OnGUI(string searchContext)
        {
            InteractionSettings config = InteractionSettings.Instance;

            EditorGUI.BeginChangeCheck();

            config.MakeGrabbablesKinematic = GUILayout.Toggle(config.MakeGrabbablesKinematic, "Grabbable objects have physics disabled by default", BuilderEditorStyles.Toggle);

            if (EditorGUI.EndChangeCheck())
            {
                InteractionSettings.Instance.Save();
            }
        }
    }
}
