using System;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UndoRedo;

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
                Guid guid = Guid.NewGuid();

                RevertableChangesHandler.Do(new ProcessCommand(
                    () => {
                        config.CreateTag(newLabel, guid);
                        EditorUtility.SetDirty(config);
                    },
                    () => {
                        config.RemoveTag(guid);
                        EditorUtility.SetDirty(config);
                    }
                    ));

                newLabel = "";
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            foreach (SceneObjectTags.Tag tag in config.Tags)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Delete"))
                {
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => {
                            config.RemoveTag(tag.Guid);
                            EditorUtility.SetDirty(config);
                        },
                        () => {
                            config.CreateTag(tag.Label, tag.Guid);
                            EditorUtility.SetDirty(config);
                        }
                        ));
                    config.RemoveTag(tag.Guid);
                    EditorUtility.SetDirty(config);
                    break;
                }

                string label = tag.Label;
                string newLabel = EditorGUILayout.TextField(label);

                if(string.IsNullOrEmpty(newLabel) == false && newLabel != label)
                {
                    RevertableChangesHandler.Do(new ProcessCommand(
                        () => {
                            config.RenameTag(tag, newLabel);
                            EditorUtility.SetDirty(config);
                        },
                        () => {
                            config.RenameTag(tag, label);
                            EditorUtility.SetDirty(config);
                        }
                        ));

                    config.RenameTag(tag, newLabel);
                    EditorUtility.SetDirty(config);
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
