using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.ProjectSettings
{
    /// <summary>
    /// Settings section to manage groups that scene objects can belong to.
    /// </summary>
    public class SceneObjectGroupsSettingsSection : IProjectSettingsSection
    {
        private string newLabel = "";
        private Dictionary<SceneObjectGroups.SceneObjectGroup, bool> foldoutStatus = new Dictionary<SceneObjectGroups.SceneObjectGroup, bool>();
        private static readonly EditorIcon deleteIcon = new EditorIcon("icon_delete");

        /// <inheritdoc/>
        public string Title => "Groups in Project";

        /// <inheritdoc/>
        public Type TargetPageProvider => typeof(SceneObjectGroupsSettingsProvider);

        /// <inheritdoc/>
        public int Priority => 64;

        /// <inheritdoc/>
        public void OnGUI(string searchContext)
        {
            SceneObjectGroups config = SceneObjectGroups.Instance;

            // Create new label
            GUILayout.BeginHorizontal();
            newLabel = EditorGUILayout.TextField(newLabel);

            EditorGUI.BeginDisabledGroup(config.CanCreateGroup(newLabel) == false);
            if (GUILayout.Button("Create Group", GUILayout.ExpandWidth(false)))
            {
                Guid guid = Guid.NewGuid();

                Undo.RecordObject(config, "Created group");
                config.CreateGroup(newLabel, guid);
                EditorUtility.SetDirty(config);

                GUI.FocusControl("");
                newLabel = "";
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // List all groups
            foreach (SceneObjectGroups.SceneObjectGroup group in config.Groups)
            {
                if (foldoutStatus.ContainsKey(group) == false)
                {
                    foldoutStatus.Add(group, false);
                }

                IEnumerable<ISceneObject> objectsInGroup = new List<ISceneObject>();

                if (RuntimeConfigurator.Exists)
                {
                    objectsInGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(group.Guid);
                }

                GUILayout.BeginHorizontal();

                // Foldout
                EditorGUI.BeginDisabledGroup(objectsInGroup.Count() == 0);

                foldoutStatus[group] = EditorGUILayout.Foldout(foldoutStatus[group], "");
                EditorGUI.EndDisabledGroup();


                string label = group.Label;

                // Label field
                EditorGUI.BeginChangeCheck();

                string newLabel = EditorGUILayout.TextField(label);

                if (EditorGUI.EndChangeCheck())
                {
                    if (string.IsNullOrEmpty(newLabel) == false && newLabel != label)
                    {
                        Undo.RecordObject(config, "Renamed group");
                        config.RenameGroup(group, newLabel);
                        EditorUtility.SetDirty(config);
                    }
                }

                // Delete button
                if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.SingleLineHeight)))
                {
                    Undo.RecordObject(config, "Deleted group");
                    config.RemoveGroup(group.Guid);
                    EditorUtility.SetDirty(config);
                    break;
                }

                // Objects in scene
                GUILayout.Label($"{objectsInGroup.Count()} objects in scene");

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(EditorDrawingHelper.VerticalSpacing);

                if (foldoutStatus[group])
                {
                    foreach (ISceneObject sceneObject in objectsInGroup)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                        if (GUILayout.Button("Show", GUILayout.ExpandWidth(false)))
                        {
                            EditorGUIUtility.PingObject(sceneObject.GameObject);
                        }

                        GUILayout.Label($"{sceneObject.GameObject.name} - uid: {sceneObject.Guid}");

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
}
