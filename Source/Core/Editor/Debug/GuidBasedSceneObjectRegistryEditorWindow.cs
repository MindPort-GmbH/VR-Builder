using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.Debug
{
    public class GuidBasedSceneObjectRegistryEditorWindow : EditorWindow
    {
        private Dictionary<Guid, bool> foldoutStatus = new Dictionary<Guid, bool>();

        private void OnEnable()
        {
            titleContent = new GUIContent("Scene Object Registry");
        }


        private void OnGUI()
        {
            GuidBasedSceneObjectRegistry sceneObjectRegistry = RuntimeConfigurator.Configuration.SceneObjectRegistry as GuidBasedSceneObjectRegistry;

            if (sceneObjectRegistry == null)
            {
                GUILayout.Label("Scene object registry is either incompatible with this debug tool or null.");
                return;
            }

            if (GUILayout.Button("Rebuild"))
            {
                sceneObjectRegistry.DebugRebuild();
            }

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

                IEnumerable<ISceneObject> objectsInGroup = sceneObjectRegistry.GetObjects(guid);

                // Foldout
                EditorGUI.BeginDisabledGroup(objectsInGroup.Count() == 0);
                string label = SceneObjectGroups.Instance.GetLabel(guid);

                if (string.IsNullOrEmpty(label))
                {
                    //RichText on Foldout breaks the Foldout may be use UIToolkit for NonUniqueSceneObjectRegistryEditorWindow in the future
                    label = SceneObjectGroups.UniqueGuidName;
                }

                foldoutStatus[guid] = EditorGUILayout.Foldout(foldoutStatus[guid], $"({objectsInGroup.Count()}) {label} - {guid.ToString()}");
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(EditorDrawingHelper.VerticalSpacing);

                if (foldoutStatus[guid])
                {
                    foreach (ISceneObject sceneObject in objectsInGroup)
                    {
                        // Check if the object has just been destroyed and throws a missing
                        // reference exception. Checking for null still throws the exception.
                        try
                        {
                            GameObject gameObject = sceneObject.GameObject;
                        }
                        catch (MissingReferenceException)
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
