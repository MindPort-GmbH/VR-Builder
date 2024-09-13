using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Editor.UI
{
    public abstract class ComponentSettingsSection<TObject, TSettings> : IProjectSettingsSection where TObject : UnityEngine.Object where TSettings : ComponentSettings<TObject, TSettings>, new()
    {
        public abstract string Title { get; }

        public abstract string Description { get; }

        public Type TargetPageProvider => typeof(ComponentSettingsProvider);

        public abstract int Priority { get; }

        private UnityEditor.Editor editor;
        private TSettings settings;

        public void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            GUIStyle labelStyle = BuilderEditorStyles.ApplyPadding(BuilderEditorStyles.Paragraph, 0);
            GUILayout.Label(Description, labelStyle);
            EditorGUILayout.Space();

            editor.OnInspectorGUI();

            EditorGUILayout.Space(20f);

            if (GUILayout.Button("Apply settings in current scene"))
            {
                TObject[] objects = Resources.FindObjectsOfTypeAll<TObject>();

                foreach (TObject snapZone in objects)
                {
                    GetSettingsInstance().ApplySettings(snapZone);
                }
            }
        }

        public ComponentSettingsSection()
        {
            editor = UnityEditor.Editor.CreateEditor(GetSettingsInstance());
        }

        private TSettings GetSettingsInstance()
        {
            if (settings != null)
            {
                return settings;
            }

            Type closedGenericType = typeof(SettingsObject<>).MakeGenericType(typeof(TSettings));

            PropertyInfo instanceProperty = closedGenericType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);

            if (instanceProperty != null)
            {
                return instanceProperty.GetValue(null) as TSettings;
            }

            return null;
        }
    }
}
