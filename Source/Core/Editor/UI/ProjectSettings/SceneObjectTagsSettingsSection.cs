using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Editor.Settings;

namespace VRBuilder.Editor.UI
{
    public class SceneObjectTagsSettingsSection : IProjectSettingsSection
    {
        private string newLabel = "";

        public string Title => "Scene Object Tags";

        public Type TargetPageProvider => typeof(SceneObjectTagsSettingsProvider);

        public int Priority => 64;

        public void OnGUI(string searchContext)
        {
            SceneObjectTags config = SceneObjectTags.Instance;

            GUILayout.BeginHorizontal();
            newLabel = EditorGUILayout.TextField(newLabel);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newLabel));
            if (GUILayout.Button("Create Tag"))
            {
                config.CreateTag(newLabel);
                newLabel = "";
                EditorUtility.SetDirty(config);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            foreach (Guid tag in config.Tags)
            {
                EditorGUILayout.LabelField(config.GetLabel(tag));
            }
        }
    }
}
