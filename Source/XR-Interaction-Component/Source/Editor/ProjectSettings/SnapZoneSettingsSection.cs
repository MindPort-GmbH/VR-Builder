using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Editor.XRInteraction;
using VRBuilder.XRInteraction;

namespace VRBuilder.Editor.UI
{
    public class SnapZoneSettingsSection : IProjectSettingsSection
    {
        public string Title => "Snap Zones";

        public Type TargetPageProvider => typeof(ComponentSettingsProvider);

        public int Priority => 1000;

        private UnityEditor.Editor editor;

        public void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            GUIStyle labelStyle = BuilderEditorStyles.ApplyPadding(BuilderEditorStyles.Paragraph, 0);
            GUILayout.Label("These settings help you to configure Snap Zones within your scenes. You can define colors and other values that will be set to Snap Zones created by clicking the 'Create Snap Zone' button of a Snappable Property.", labelStyle);
            EditorGUILayout.Space();

            editor.OnInspectorGUI();

            EditorGUILayout.Space(20f);

            if (GUILayout.Button("Apply settings in current scene"))
            {
                SnapZone[] snapZones = Resources.FindObjectsOfTypeAll<SnapZone>();

                foreach (SnapZone snapZone in snapZones)
                {
                    SnapZoneSettings.Instance.ApplySettings(snapZone);
                }
            }
        }

        public SnapZoneSettingsSection()
        {
            editor = UnityEditor.Editor.CreateEditor(SnapZoneSettings.Instance);
        }
    }
}