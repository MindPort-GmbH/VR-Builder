// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Editor window that hosts one Step Inspector panel. Every panel opens in its own
    /// instance so users can dock them via Unity's native dock system however they want.
    /// </summary>
    public sealed class DetachedPanelWindow : EditorWindow
    {
        [SerializeField] private string panelId;

        private VisualElement contentRoot;
        private bool registered;

        public string PanelId => panelId;

        /// <summary>
        /// Focuses an existing window for <paramref name="panelId"/> if one is open,
        /// otherwise spawns a fresh instance.
        /// </summary>
        internal static DetachedPanelWindow OpenOrFocus(string panelId)
        {
            foreach (DetachedPanelWindow existing in Resources.FindObjectsOfTypeAll<DetachedPanelWindow>())
            {
                if (existing != null && existing.panelId == panelId)
                {
                    existing.Focus();
                    return existing;
                }
            }

            return Create(panelId);
        }

        /// <summary>
        /// Always spawns a fresh window for <paramref name="panelId"/>, even when another
        /// instance with the same id is already open. Lets users stack multiple Transitions /
        /// Behaviors / etc. windows side-by-side.
        /// </summary>
        internal static DetachedPanelWindow OpenNew(string panelId)
        {
            return Create(panelId);
        }

        /// <summary>
        /// Creates a fresh instance but DOES NOT call <see cref="EditorWindow.Show"/>.
        /// Use this when the caller will dock the window into an existing container
        /// itself (e.g. via <see cref="WindowDockingHelper.DockAsTab"/>) — that path
        /// hosts the window without ever creating a stray floating ContainerWindow.
        /// </summary>
        internal static DetachedPanelWindow CreateForDocking(string panelId)
        {
            DetachedPanelWindow window = CreateInstance<DetachedPanelWindow>();
            window.panelId = panelId;
            window.titleContent = new GUIContent(TitleFor(panelId));
            window.minSize = new Vector2(420f, 320f);
            // No Show() here — caller docks via AddTab.
            return window;
        }

        private static DetachedPanelWindow Create(string panelId)
        {
            DetachedPanelWindow window = CreateInstance<DetachedPanelWindow>();
            window.panelId = panelId;
            window.titleContent = new GUIContent(TitleFor(panelId));
            window.minSize = new Vector2(420f, 320f);
            window.Show();
            window.Focus();
            return window;
        }

        private void OnEnable()
        {
            StepInspectorUITKEditingStrategy.Acquire();
            registered = true;

            StepSelectionService.SelectionChanged += OnSelectionChanged;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            StepSelectionService.SelectionChanged -= OnSelectionChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            if (registered)
            {
                StepInspectorUITKEditingStrategy.Release();
                registered = false;
            }
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.name = "step-inspector-root";

            StyleSheet sheet = StepInspectorAssetLoader.LoadMainStyleSheet();
            if (sheet != null)
            {
                root.styleSheets.Add(sheet);
            }

            contentRoot = new VisualElement { name = "step-inspector-content" };
            contentRoot.style.flexGrow = 1f;
            root.Add(contentRoot);

            titleContent = new GUIContent(TitleFor(panelId));
            Rebuild();
        }

        private void OnSelectionChanged(IStep step, IChapter chapter, IProcess process)
        {
            // Selection / structural changes invalidate the previous validation report; re-run
            // before Rebuild so DecorateMember / DecorateEntityHeader read fresh entries.
            RunValidationIfAllowed();
            Rebuild();
        }

        private void OnUndoRedoPerformed()
        {
            RunValidationIfAllowed();
            Rebuild();
        }

        private void OnFocus()
        {
            // Matches the legacy IMGUI StepWindow.OnFocus contract: refocusing the inspector
            // forces a fresh validation pass so icons and Fix UI reflect current scene state
            // (e.g. a referenced scene object was deleted while the window was unfocused).
            // contentRoot is null until CreateGUI() has run; skip until then.
            if (contentRoot == null) return;

            RunValidationIfAllowed();
            Rebuild();
        }

        private static void RunValidationIfAllowed()
        {
            IStep step = StepSelectionService.CurrentStep;
            if (step?.Data == null) return;

            IProcess process = StepSelectionService.CurrentProcess;
            if (process == null) return;

            try
            {
                if (EditorConfigurator.Instance?.Validation == null) return;
                if (EditorConfigurator.Instance.Validation.IsAllowedToValidate() == false) return;

                EditorConfigurator.Instance.Validation.Validate(step.Data, process);
            }
            catch
            {
                // Validation must never break the inspector — overlay falls back to "no entries".
            }
        }

        private void Rebuild()
        {
            if (contentRoot == null)
            {
                return;
            }

            contentRoot.Clear();

            IStep currentStep = StepSelectionService.CurrentStep;
            if (currentStep?.Data == null)
            {
                Label empty = new Label("Select a step in the Process Editor.");
                empty.style.unityTextAlign = TextAnchor.MiddleCenter;
                empty.style.flexGrow = 1f;
                contentRoot.Add(empty);
                return;
            }

            StepElementDrawer drawer = ElementDrawerLocator.GetDrawerForValue(
                currentStep.Data, typeof(Step.EntityData)) as StepElementDrawer;

            if (drawer == null)
            {
                contentRoot.Add(new Label("(no drawer registered for Step.EntityData)"));
                return;
            }

            VisualElement panelBody = drawer.BuildPanel(panelId, (Step.EntityData)currentStep.Data, _ => Repaint());
            if (panelBody == null)
            {
                contentRoot.Add(new Label($"(no content for panel '{panelId}')"));
                return;
            }

            PanelHost host = new PanelHost(
                panelId,
                TitleFor(panelId),
                panelBody,
                onDuplicate: () => OpenNew(panelId));
            host.style.flexGrow = 1f;
            AppendValidationIcon(host, currentStep.Data, panelId);
            contentRoot.Add(host);
        }

        private static void AppendValidationIcon(PanelHost host, IStepData stepData, string panelId)
        {
            UnityEngine.UIElements.Image icon = panelId switch
            {
                PanelIds.Behaviors => Validation.ValidationOverlay.BuildBehaviorsTabIcon(stepData),
                PanelIds.Transitions => Validation.ValidationOverlay.BuildTransitionsTabIcon(stepData),
                _ => null,
            };

            if (icon == null) return;

            icon.AddToClassList("vrb-validation-icon--panel");
            host.Header.Insert(0, icon);
        }

        private static string TitleFor(string id)
        {
            switch (id)
            {
                case PanelIds.Header:      return "Step";
                case PanelIds.Behaviors:   return "Behaviors";
                case PanelIds.Transitions: return "Transitions";
                case PanelIds.Unlocked:    return "Unlocked Objects";
                default: return string.IsNullOrEmpty(id) ? "Panel" : id;
            }
        }
    }
}
