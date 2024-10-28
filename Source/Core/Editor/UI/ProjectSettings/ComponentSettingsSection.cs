using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Settings section holding global settings for a specified scene component.
    /// </summary>
    /// <typeparam name="TObject">The component that can be configured by this section.</typeparam>
    /// <typeparam name="TSettings">The scriptable object holding the global settings.</typeparam>
    public abstract class ComponentSettingsSection<TObject, TSettings> : IProjectSettingsSection where TObject : UnityEngine.Object where TSettings : ComponentSettings<TObject, TSettings>, new()
    {
        /// <inheritdoc/>                
        public abstract string Title { get; }

        /// <summary>
        /// Description of this settings section. Will be displayed below the title, before the settings in the scriptable object.
        /// </summary>
        public abstract string Description { get; }

        /// <inheritdoc/>        
        public Type TargetPageProvider => typeof(ComponentSettingsProvider);

        /// <inheritdoc/>        
        public abstract int Priority { get; }

        private UnityEditor.Editor editor;
        private TSettings settings;

        /// <inheritdoc/>        
        public void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            GUIStyle labelStyle = BuilderEditorStyles.ApplyPadding(BuilderEditorStyles.Paragraph, 0);
            GUILayout.Label(Description, labelStyle);
            EditorGUILayout.Space();

            editor.OnInspectorGUI();

            if (GUILayout.Button("Apply settings in current scene"))
            {
                TObject[] objects = Resources.FindObjectsOfTypeAll<TObject>();

                foreach (TObject snapZone in objects)
                {
                    GetSettingsInstance().ApplySettings(snapZone);
                }
            }

            EditorGUILayout.Space(20f);
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
