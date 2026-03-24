// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.NewStepInspector.Tabs;
using VRBuilder.Core.Editor.UndoRedo;

namespace VRBuilder.Core.Editor.UI.NewStepInspector
{
    /// <summary>
    /// Thin orchestrator window for the new UIToolkit Step Inspector.
    /// Delegates all UI building and data display to tab classes.
    /// </summary>
    public sealed class StepInspectorUITKWindow : EditorWindow
    {
        private const string Title = "Step Inspector (New UITK)";
        private const string UxmlGuid = ""; // Set after importing, or use path
        private const double PollIntervalSeconds = 0.1;

        private IStep step;
        private IChapter chapter;
        private IProcess process;

        private double lastPollTime;
        private bool suppressCallbacks;

        private HelpBox noStepHelpBox;
        private TextField stepNameField;
        private Label contextLabel;

        private StepInspectorTabBar tabBar;
        private IVisualElementScheduledItem pollTask;

        [MenuItem("Tools/VR Builder/New Step Inspector (UITK)")]
        public static void OpenWindow()
        {
            StepInspectorUITKWindow window = GetWindow<StepInspectorUITKWindow>(Title);
            window.minSize = new Vector2(620f, 420f);
            window.Show();
        }

        public static void OpenOrFocusAndBind(IStep selectedStep, IChapter selectedChapter, IProcess selectedProcess, bool focus)
        {
            StepInspectorUITKWindow window = GetWindow<StepInspectorUITKWindow>(Title, focus);
            window.minSize = new Vector2(620f, 420f);
            window.Show();
            window.SetSelection(selectedStep, selectedChapter, selectedProcess, true);
            if (focus) window.Focus();
        }

        public static void BindOpenWindows(IStep selectedStep, IChapter selectedChapter, IProcess selectedProcess)
        {
            foreach (StepInspectorUITKWindow window in Resources.FindObjectsOfTypeAll<StepInspectorUITKWindow>())
            {
                window.SetSelection(selectedStep, selectedChapter, selectedProcess, true);
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.projectChanged += OnExternalDataChanged;
            EditorApplication.hierarchyChanged += OnExternalDataChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.projectChanged -= OnExternalDataChanged;
            EditorApplication.hierarchyChanged -= OnExternalDataChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;

            pollTask?.Pause();
            pollTask = null;
            tabBar?.Dispose();
        }

        private void OnEditorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - lastPollTime < PollIntervalSeconds) return;
            lastPollTime = now;
            BindCurrentStep(force: false);
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            // Load USS
            string ussPath = FindAssetPath("StepInspectorUITK", "uss");
            if (!string.IsNullOrEmpty(ussPath))
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                if (styleSheet != null) rootVisualElement.styleSheets.Add(styleSheet);
            }

            // Load UXML
            string uxmlPath = FindAssetPath("StepInspectorUITK", "uxml");
            if (!string.IsNullOrEmpty(uxmlPath))
            {
                VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                if (uxml != null) uxml.CloneTree(rootVisualElement);
            }

            rootVisualElement.AddToClassList("step-inspector__root");

            // Query header elements
            Label titleLabel = rootVisualElement.Q<Label>("header-title");
            if (titleLabel != null) titleLabel.text = Title;

            // Query context elements
            VisualElement helpBoxContainer = rootVisualElement.Q("no-step-help-box-container");
            noStepHelpBox = new HelpBox("No step selected. Select a step in the Process Editor.", HelpBoxMessageType.Info);
            helpBoxContainer?.Add(noStepHelpBox);

            stepNameField = rootVisualElement.Q<TextField>("step-name-field");
            if (stepNameField != null)
            {
                stepNameField.isDelayed = true;
                stepNameField.RegisterValueChangedCallback(evt => ApplyStepName(evt.newValue));
            }

            contextLabel = rootVisualElement.Q<Label>("context-label");

            // Build tabs
            List<IStepInspectorTab> tabs = new List<IStepInspectorTab>
            {
                new BehaviorsTab(),
                new TransitionsTab(),
                new UnlockedObjectsTab()
            };

            tabBar = new StepInspectorTabBar(tabs);
            VisualElement tabContent = tabBar.Build();

            VisualElement tabContainer = rootVisualElement.Q("tab-content-container");
            if (tabContainer != null)
            {
                tabContainer.Add(tabContent);
            }
            else
            {
                rootVisualElement.Add(tabContent);
            }

            BindCurrentStep(force: true);

            pollTask?.Pause();
            pollTask = rootVisualElement.schedule.Execute(PollEditorState).Every(250);
        }

        private void BindCurrentStep(bool force)
        {
            IChapter currentChapter = GlobalEditorHandler.GetCurrentChapter() ?? chapter;
            IStep currentStep = currentChapter?.ChapterMetadata?.LastSelectedStep ?? step;
            IProcess currentProcess = GlobalEditorHandler.GetCurrentProcess() ?? process;

            SetSelection(currentStep, currentChapter, currentProcess, force);
        }

        private void SetSelection(IStep selectedStep, IChapter selectedChapter, IProcess selectedProcess, bool forceRefresh)
        {
            bool changed = !ReferenceEquals(step, selectedStep)
                || !ReferenceEquals(chapter, selectedChapter)
                || !ReferenceEquals(process, selectedProcess);

            if (!changed && !forceRefresh) return;

            step = selectedStep;
            chapter = selectedChapter;
            process = selectedProcess;

            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshContext();
            tabBar?.RefreshAll(step, chapter, process);
        }

        private void RefreshContext()
        {
            bool hasStep = step != null;

            if (noStepHelpBox != null)
                noStepHelpBox.style.display = hasStep ? DisplayStyle.None : DisplayStyle.Flex;

            stepNameField?.SetEnabled(hasStep);

            suppressCallbacks = true;
            stepNameField?.SetValueWithoutNotify(hasStep ? step.Data.Name ?? string.Empty : string.Empty);
            suppressCallbacks = false;

            if (contextLabel != null)
            {
                if (hasStep)
                {
                    string processName = process?.Data?.Name ?? "<No Process>";
                    string chapterName = chapter?.Data?.Name ?? "<No Chapter>";
                    contextLabel.text = $"Process: {processName}   |   Chapter: {chapterName}";
                }
                else
                {
                    contextLabel.text = "No step selected.";
                }
            }
        }

        private void ApplyStepName(string newName)
        {
            if (suppressCallbacks || step == null) return;

            string oldName = step.Data.Name ?? string.Empty;
            string normalized = newName ?? string.Empty;
            if (oldName == normalized) return;

            ExecuteMutation(
                () => step.Data.SetName(normalized),
                () => step.Data.SetName(oldName));
        }

        private void PollEditorState()
        {
            if (step == null) return;

            if (stepNameField != null && stepNameField.value != (step.Data.Name ?? string.Empty))
            {
                suppressCallbacks = true;
                stepNameField.SetValueWithoutNotify(step.Data.Name ?? string.Empty);
                suppressCallbacks = false;
            }
        }

        private void ExecuteMutation(Action doAction, Action undoAction)
        {
            RevertableChangesHandler.Do(new ProcessCommand(doAction, undoAction));
        }

        private void OnUndoRedoPerformed() => RefreshAll();
        private void OnExternalDataChanged() => RefreshAll();
        private void OnPlayModeStateChanged(PlayModeStateChange _) => RefreshAll();

        /// <summary>
        /// Finds the asset path for a file with the given name and extension
        /// within the NewStepInspector directory.
        /// </summary>
        private static string FindAssetPath(string fileName, string extension)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith($".{extension}") && path.Contains("NewStepInspector"))
                {
                    return path;
                }
            }
            return null;
        }
    }
}
