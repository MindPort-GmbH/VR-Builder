// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK
{
    /// <summary>
    /// Menu entry points for the new UIToolkit-based Step Inspector.
    /// During the migration this opens via menu only and does not replace
    /// the legacy <see cref="VRBuilder.Core.Editor.UI.Windows.StepWindow"/>.
    /// </summary>
    internal static class StepInspectorMenu
    {
        private const string OpenAllMenuPath = "Tools/VR Builder/Step Inspector (UITK)";

        [MenuItem(OpenAllMenuPath, priority = 200)]
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
