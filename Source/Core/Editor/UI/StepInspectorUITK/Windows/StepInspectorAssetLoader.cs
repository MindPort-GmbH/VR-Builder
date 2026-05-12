// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    internal static class StepInspectorAssetLoader
    {
        private const string StyleSheetPath =
            "Packages/co.mindport.vrbuilder.core/Source/Core/Editor/UI/StepInspectorUITK/Resources/StepInspector.uss";

        // Used when the package is mounted via file: protocol or relocated under Assets/.
        private const string StyleSheetFallbackPath =
            "Assets/MindPort/VR Builder/Source/Core/Editor/UI/StepInspectorUITK/Resources/StepInspector.uss";

        public static StyleSheet LoadMainStyleSheet()
        {
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet == null)
            {
                sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetFallbackPath);
            }
            return sheet;
        }
    }
}
