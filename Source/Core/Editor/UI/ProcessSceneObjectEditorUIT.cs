// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using UnityEditor;
using UnityEngine.UIElements;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI
{
    [CustomEditor(typeof(ProcessSceneObjectUIT), true)]
    public class ProcessSceneObjectEditorUIT : UnityEditor.Editor
    {
        public VisualTreeAsset ManageTagsPannel;

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement myInspector = new VisualElement();

            // Load and clone a visual tree from UXML
            ManageTagsPannel.CloneTree(myInspector);

            // Return the finished inspector UI
            return myInspector;
        }
    }
}
