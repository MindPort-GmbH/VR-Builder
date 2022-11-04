// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEditor;
using UnityEngine;
using System.Reflection;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;

namespace VRBuilder.Editor.UI
{
    /// <summary>
    /// This class adds names to newly added entities.
    /// </summary>
    [CustomEditor(typeof(ProcessSceneObject))]
    internal class SceneObjectEditor : UnityEditor.Editor
    {
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
    }
}

