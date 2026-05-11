// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK
{
    public sealed class StepInspectorWindow : EditorWindow
    {
        private const string StyleSheetPath =
            "Packages/co.mindport.vrbuilder.core/Source/Core/Editor/UI/StepInspectorUITK/Resources/StepInspector.uss";

        // Used when the package is mounted via file: protocol or relocated under Assets/.
        private const string StyleSheetFallbackPath =
            "Assets/MindPort/VR Builder/Source/Core/Editor/UI/StepInspectorUITK/Resources/StepInspector.uss";

        private const double SelectionPollIntervalSeconds = 0.25;

        private VisualElement contentRoot;
        private IStep boundStep;
        private double lastPollTime;

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "step-inspector-root";

            StyleSheet styleSheet = LoadStyleSheet();
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            contentRoot = new VisualElement { name = "step-inspector-content" };
            contentRoot.style.flexGrow = 1f;
            root.Add(contentRoot);

            Rebind(force: true);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        private void OnEditorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - lastPollTime < SelectionPollIntervalSeconds)
            {
                return;
            }

            lastPollTime = now;
            Rebind(force: false);
        }

        private void OnUndoRedoPerformed()
        {
            Rebind(force: true);
        }

        // Phase 4 will replace polling with StepSelectionService event subscription.
        private void Rebind(bool force)
        {
            if (contentRoot == null)
            {
                return;
            }

            IStep currentStep = GlobalEditorHandler.GetCurrentChapter()?.ChapterMetadata?.LastSelectedStep;

            if (force == false && ReferenceEquals(currentStep, boundStep))
            {
                return;
            }

            boundStep = currentStep;
            contentRoot.Clear();

            if (currentStep?.Data == null)
            {
                Label empty = new Label("Select a step in the Process Editor.")
                {
                    name = "step-inspector-placeholder"
                };
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.flexGrow = 1f;
                contentRoot.Add(empty);
                return;
            }

            IElementDrawer drawer = ElementDrawerLocator.GetDrawerForValue(currentStep.Data, typeof(Step.EntityData));
            if (drawer == null)
            {
                contentRoot.Add(new Label("(no drawer registered for Step.EntityData)"));
                return;
            }

            VisualElement stepView = drawer.CreateElement(
                value: currentStep.Data,
                changeCallback: _ => Repaint(),
                label: GUIContent.none);

            if (stepView != null)
            {
                contentRoot.Add(stepView);
            }
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
