// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK
{
    public sealed class StepInspectorWindow : EditorWindow
    {
        private const string StyleSheetPath =
            "Packages/co.mindport.vrbuilder.core/Source/Core/Editor/UI/StepInspectorUITK/Resources/StepInspector.uss";

        // Used when the package is mounted via file: protocol or relocated under Assets/.
        private const string StyleSheetFallbackPath =
            "Assets/MindPort/VR Builder/Source/Core/Editor/UI/StepInspectorUITK/Resources/StepInspector.uss";

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "step-inspector-root";

            StyleSheet styleSheet = LoadStyleSheet();
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            Label placeholder = new Label("Step Inspector (UITK) — bootstrapping (Phase 0)")
            {
                name = "step-inspector-placeholder"
            };
            placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholder.style.flexGrow = 1f;
            root.Add(placeholder);
        }

        private static StyleSheet LoadStyleSheet()
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
