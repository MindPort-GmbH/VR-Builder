using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// Settings section to manage tags that can be attached to scene objects.
    /// </summary>
    public class SceneObjectTagsSettingsSection : IProjectSettingsSection
    {
        private string newLabel = "";
        private Dictionary<SceneObjectTags.Tag, bool> foldoutStatus = new Dictionary<SceneObjectTags.Tag, bool>();
        private static readonly EditorIcon deleteIcon = new EditorIcon("icon_delete");

        /// <inheritdoc/>
        public string Title => "Tags in Project";

        /// <inheritdoc/>
        public Type TargetPageProvider => typeof(SceneObjectTagsSettingsProvider);

        /// <inheritdoc/>
        public int Priority => 64;

        /// <inheritdoc/>
        public void OnGUI(string searchContext)
        {
            SceneObjectTags config = SceneObjectTags.Instance;

            // Create new label
            GUILayout.BeginHorizontal();
            newLabel = EditorGUILayout.TextField(newLabel);

            EditorGUI.BeginDisabledGroup(config.CanCreateTag(newLabel) == false);
            if (GUILayout.Button("Create Tag", GUILayout.ExpandWidth(false)))
            {
                Guid guid = Guid.NewGuid();

                Undo.RecordObject(config, "Created tag");
                config.CreateTag(newLabel, guid);
                EditorUtility.SetDirty(config);

                GUI.FocusControl("");
                newLabel = "";
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // List all tags
            foreach (SceneObjectTags.Tag tag in config.Tags)
            {
                if (foldoutStatus.ContainsKey(tag) == false)
                {
                    foldoutStatus.Add(tag, false);
                }

                IEnumerable<ISceneObject> objectsWithTag = new List<ISceneObject>();

                if (RuntimeConfigurator.Exists)
                {
                    objectsWithTag = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(tag.Guid);
                }

                GUILayout.BeginHorizontal();

                // Foldout
                EditorGUI.BeginDisabledGroup(objectsWithTag.Count() == 0);

                foldoutStatus[tag] = EditorGUILayout.Foldout(foldoutStatus[tag], "");
                EditorGUI.EndDisabledGroup();


                string label = tag.Label;

                // Label field
                EditorGUI.BeginChangeCheck();

                string newLabel = EditorGUILayout.TextField(label);

                if (EditorGUI.EndChangeCheck())
                {
                    if (string.IsNullOrEmpty(newLabel) == false && newLabel != label)
                    {
                        Undo.RecordObject(config, "Renamed tag");
                        config.RenameTag(tag, newLabel);
                        EditorUtility.SetDirty(config);
                    }
                }

                // Delete button
                if (GUILayout.Button(deleteIcon.Texture, GUILayout.Height(EditorDrawingHelper.SingleLineHeight)))
                {
                    Undo.RecordObject(config, "Deleted tag");
                    config.RemoveTag(tag.Guid);
                    EditorUtility.SetDirty(config);
                    break;
                }

                // Objects in scene
                GUILayout.Label($"{objectsWithTag.Count()} objects in scene");

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(EditorDrawingHelper.VerticalSpacing);

                if (foldoutStatus[tag])
                {
                    foreach (ISceneObject sceneObject in objectsWithTag)
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
