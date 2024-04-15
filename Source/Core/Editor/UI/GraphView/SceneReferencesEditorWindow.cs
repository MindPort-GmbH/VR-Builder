using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Editor.UI.Windows
{
    public class SceneReferencesEditorWindow : EditorWindow
    {
        private ProcessSceneReferenceBase reference;

        public void SetReference(ProcessSceneReferenceBase reference)
        {
            this.reference = reference;
        }

        private void DrawLabel(Guid guidToDisplay)
        {
            string label;

            ISceneObjectRegistry registry = RuntimeConfigurator.Configuration.SceneObjectRegistry;
            if (registry.ContainsGuid(guidToDisplay))
            {
                SceneObjectGroups.SceneObjectGroup group;
                if (SceneObjectGroups.Instance.TryGetGroup(guidToDisplay, out group))
                {
                    label = group.Label;
                }
                else
                {
                    label = SceneObjectGroups.UniqueGuidNameItalic;
                }
            }
            else
            {
                label = $"{SceneObjectGroups.GuidNotRegisteredText} - {guidToDisplay}.";
            }

            GUILayout.Label($"Group: {label}");
            //GUILayout.Label($"Group: {label}", richTextLabelStyle);
        }

        private void OnGUI()
        {
            if (reference == null || reference.IsEmpty())
            {
                return;
            }

            if (reference.Guids.Count == 0)
            {
                return;
            }

            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }

            foreach (Guid guidToDisplay in reference.Guids)
            {
                IEnumerable<ISceneObject> processSceneObjectsWithGroup = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guidToDisplay);

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                DrawLabel(guidToDisplay);
                if (GUILayout.Button("Select"))
                {
                    // Select all game objects with the group in the Hierarchy
                    Selection.objects = processSceneObjectsWithGroup.Select(processSceneObject => processSceneObject.GameObject).ToArray();
                }
                if (GUILayout.Button("Remove"))
                {
                    reference.RemoveGuid(guidToDisplay);
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    return;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                foreach (ISceneObject sceneObject in processSceneObjectsWithGroup)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    GUILayout.Space(EditorDrawingHelper.IndentationWidth);
                    GUILayout.Label($"{sceneObject.GameObject.name}");
                    if (GUILayout.Button("Show"))
                    {
                        EditorGUIUtility.PingObject(sceneObject.GameObject);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
