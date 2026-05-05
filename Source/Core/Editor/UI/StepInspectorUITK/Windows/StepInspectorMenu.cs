// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK
{
    internal static class StepInspectorMenu
    {
        [MenuItem("Tools/VR Builder/Step Inspector (UITK)", priority = 200)]
        public static void OpenStepInspector()
        {
            StepInspectorWindow window = EditorWindow.GetWindow<StepInspectorWindow>(
                title: "Step Inspector",
                focus: true);

            window.minSize = new Vector2(420f, 360f);
            window.Show();
        }
    }
}
