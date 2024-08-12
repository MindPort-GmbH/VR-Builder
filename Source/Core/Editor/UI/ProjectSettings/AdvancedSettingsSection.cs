using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// Settings useful to speed up some procedures for advanced users.
    /// </summary>
    public class AdvancedSettingsSection : IProjectSettingsSection
    {
        public string Title => "Advanced";

        public Type TargetPageProvider => typeof(BuilderPageProvider);

        public int Priority => 1000;

        public void OnGUI(string searchContext)
        {
            AdvancedSettings config = AdvancedSettings.Instance;

            EditorGUI.BeginChangeCheck();

            config.ShowExpertInfo = GUILayout.Toggle(config.ShowExpertInfo, "Show object ID in the inspector of process scene objects", BuilderEditorStyles.Toggle);

            config.AutoAddProcessSceneObject = GUILayout.Toggle(config.AutoAddProcessSceneObject, "Automatically add Process Scene Object component to objects dragged in the step inspector", BuilderEditorStyles.Toggle);

            config.AutoAddProperties = GUILayout.Toggle(config.AutoAddProperties, "Automatically add required properties when referencing a scene object in the Step Inspector", BuilderEditorStyles.Toggle);

            if (EditorGUI.EndChangeCheck())
            {
                config.Save();
            }
        }
    }
}