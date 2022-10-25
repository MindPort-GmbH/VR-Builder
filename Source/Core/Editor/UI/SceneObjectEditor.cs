// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEditor;
using UnityEngine;
using System.Reflection;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;
using VRBuilder.Editor.Settings;
using System.Linq;
using System;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// This class adds names to newly added entities.
    /// </summary>
    [CustomEditor(typeof(ProcessSceneObject))]
    internal class SceneObjectEditor : UnityEditor.Editor
    {
        int selectedTagIndex = 0;

        private void OnEnable()
        {
            ISceneObject sceneObject = target as ISceneObject;
            FieldInfo fieldInfoObj = sceneObject.GetType().GetField("uniqueName", BindingFlags.NonPublic | BindingFlags.Instance);
            string uniqueName = fieldInfoObj.GetValue(sceneObject) as string;

            if (string.IsNullOrEmpty(uniqueName))
            {
                sceneObject.SetSuitableName();
            }
        }

        [MenuItem ("CONTEXT/ProcessSceneObject/Remove Process Properties", false)]
        private static void RemoveProcessProperties()
        {
            Component[] processProperties = Selection.activeGameObject.GetComponents(typeof(ProcessSceneObjectProperty));
            ISceneObject sceneObject = Selection.activeGameObject.GetComponent(typeof(ISceneObject)) as ISceneObject;

            foreach (Component processProperty in processProperties)
            {
                sceneObject.RemoveProcessProperty(processProperty, true);
            }
        }

        [MenuItem("CONTEXT/ProcessSceneObject/Remove Process Properties", true)]
        private static bool ValidateRemoveProcessProperties()
        {
            return Selection.activeGameObject.GetComponents(typeof(ProcessSceneObjectProperty)) != null;
        }

        public override void OnInspectorGUI()
        {
            ProcessSceneObject sceneObject = target as ProcessSceneObject;

            string name = EditorGUILayout.TextField("Unique Name", sceneObject.UniqueName);

            if (name != sceneObject.UniqueName)
            {
                sceneObject.ChangeUniqueName(name);
            }

            SceneObjectTags.Tag[] availableTags = SceneObjectTags.Instance.Tags.Where(tag => sceneObject.HasTag(tag.Guid) == false).ToArray();

            if (selectedTagIndex >= availableTags.Length && availableTags.Length > 0)
            {
                selectedTagIndex = availableTags.Length - 1;
            }

            EditorGUI.BeginDisabledGroup(availableTags.Length == 0);
            selectedTagIndex = EditorGUILayout.Popup(selectedTagIndex, availableTags.Select(tag => tag.Label).ToArray());

            if(GUILayout.Button("Add tag"))
            {
                sceneObject.AddTag(availableTags[selectedTagIndex].Guid);
            }
            EditorGUI.EndDisabledGroup();

            foreach(Guid tag in sceneObject.Tags)
            {
                if(SceneObjectTags.Instance.TagExists(tag) == false)
                {
                    sceneObject.RemoveTag(tag);
                    break;
                }

                EditorGUILayout.BeginHorizontal();                
                EditorGUILayout.LabelField(SceneObjectTags.Instance.GetLabel(tag));

                if(GUILayout.Button("X"))
                {
                    sceneObject.RemoveTag(tag);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}

