using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Editor.UI;

namespace VRBuilder.Editor.Debugging
{
    public class NonUniqueSceneObjectRegistryEditorWindow : EditorWindow
    {
        private NonUniqueSceneObjectRegistry sceneObjectRegistry;
        private Dictionary<Guid, bool> foldoutStatus = new Dictionary<Guid, bool>();

        private void OnEnable()
        {
            titleContent = new GUIContent("Scene Object Registry");

            try
            {
                sceneObjectRegistry = RuntimeConfigurator.Configuration.SceneObjectRegistry as NonUniqueSceneObjectRegistry;
            }
            catch
            {
            }
        }


        private void OnGUI()
        {
            if (sceneObjectRegistry == null)
            {
                return;
            }

            if (sceneObjectRegistry.RegisteredGuids.Count() == 0)
            {
                GUILayout.Label("No Guids registered.");
                return;
            }

            foreach (Guid guid in sceneObjectRegistry.RegisteredGuids)
            {
                if (foldoutStatus.ContainsKey(guid) == false)
                {
                    foldoutStatus.Add(guid, false);
                }

                IEnumerable<ISceneObject> objectsWithTag = sceneObjectRegistry.GetByTag(guid);

                // Foldout
                EditorGUI.BeginDisabledGroup(objectsWithTag.Count() == 0);
                string label = SceneObjectTags.Instance.GetLabel(guid);

                if (string.IsNullOrEmpty(label))
                {
                    label = $"[NO LABEL]";
                }
                foldoutStatus[guid] = EditorGUILayout.Foldout(foldoutStatus[guid], $"({objectsWithTag.Count()}) {label} - {guid.ToString()}");
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(EditorDrawingHelper.VerticalSpacing);

                if (foldoutStatus[guid])
                {
                    foreach (ISceneObject sceneObject in objectsWithTag)
                    {
                        // Check if the object has just been destroyed and throws a missing
                        // reference exception. Checking for null still throws the exception.
                        try
                        {
                            GameObject gameObject = sceneObject.GameObject;
                        }
                        catch (MissingReferenceException e)
                        {
                            continue;
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(EditorDrawingHelper.IndentationWidth);

                        if (GUILayout.Button("Show", GUILayout.ExpandWidth(false)))
                        {
                            EditorGUIUtility.PingObject(sceneObject.GameObject);
                        }

                        GUILayout.Label($"{sceneObject.GameObject.name}");

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
}
