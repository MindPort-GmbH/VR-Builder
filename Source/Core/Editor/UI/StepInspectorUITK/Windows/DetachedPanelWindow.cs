using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Editor window that hosts one Step Inspector panel. Every panel opens in its own
    /// instance so users can dock them via Unity's native dock system however they want.
    /// </summary>
    public class DetachedPanelWindow : EditorWindow
    {
        [SerializeField] private string panelId;

        private VisualElement contentRoot;

        public string PanelId => panelId;

        /// <summary>
        /// Panel id a subclass renders when the window is created without an explicit id —
        /// e.g. instantiated by Unity's native "Add Tab" menu, which calls the parameterless
        /// <see cref="ScriptableObject.CreateInstance(System.Type)"/>. The base class itself
        /// has no default (it is configured via the static factory methods below).
        /// </summary>
        protected virtual string DefaultPanelId => null;

        /// <summary>
        /// Backfills <see cref="panelId"/> from <see cref="DefaultPanelId"/> when no id was set.
        /// Lets the typed subclasses (Step / Behaviors / Transitions / Unlocked Objects) render
        /// the right panel after being spawned arg-less by the Add Tab menu.
        /// </summary>
        private void EnsurePanelId()
        {
            if (string.IsNullOrEmpty(panelId))
            {
                panelId = DefaultPanelId;
            }
        }

        /// <summary>
        /// Adds the four Step Inspector panels to Unity's right-click "Add Tab" menu of any
        /// VR Builder inspector window. Unity only lists a window type here if the focused
        /// window returns it from this override (custom windows are not auto-discovered), and
        /// instantiates a fresh copy per click — so picking the same entry twice yields two
        /// independent tabs.
        /// </summary>
        public override IEnumerable<Type> GetExtraPaneTypes()
        {
            yield return typeof(StepTabWindow);
            yield return typeof(BehaviorsTabWindow);
            yield return typeof(TransitionsTabWindow);
            yield return typeof(UnlockedObjectsTabWindow);
        }

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
            EnsurePanelId();
            StepSelectionService.SelectionChanged += OnSelectionChanged;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            StepSelectionService.SelectionChanged -= OnSelectionChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        private void CreateGUI()
        {
            EnsurePanelId();

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
            // Defensive: never tear down the visual tree while a drag is in flight. Today
            // nothing fires SelectionChanged mid-drag, but a future decorator could — and
            // rebuilding while DragDropBinder holds references to row elements would corrupt
            // pointer capture.
            if (DragSession.IsActive)
            {
                return;
            }

            // Fast path: a same-list reorder just happened. Move the row VisualElement in
            // place instead of clearing and rebuilding the whole panel — that's the visible
            // flicker users complained about. Validation results don't depend on order, so
            // skipping the validation re-run here is fine; undo/redo still goes through the
            // full Rebuild path. Don't clear the hint — ListMoveCommand owns its lifetime
            // and clears it after every subscriber has run.
            ReorderHint hint = DragSession.PendingReorder;
            if (hint != null && TryApplyReorderInPlace(hint))
            {
                return;
            }

            // Selection / structural changes invalidate the previous validation report; re-run
            // before Rebuild so DecorateMember / DecorateEntityHeader read fresh entries.
            RunValidationIfAllowed();
            Rebuild();
        }

        /// <summary>
        /// Reorders the row VisualElement matching <paramref name="hint"/> inside its
        /// container without touching the rest of the tree. Returns false if the affected
        /// container can't be located in THIS window — caller falls back to full Rebuild.
        /// </summary>
        private bool TryApplyReorderInPlace(ReorderHint hint)
        {
            if (contentRoot == null)
            {
                return false;
            }

            VisualElement container = DropTargetRegistry.FindContainerForList(hint.List);
            if (container == null)
            {
                return false;
            }

            // Confirm the container belongs to this window (multiple DetachedPanelWindows
            // share the same DragSession; only the one hosting the affected list should act).
            if (!IsDescendantOfThisWindow(container))
            {
                return false;
            }

            int childCount = container.childCount;
            if (hint.FromIndex < 0 || hint.FromIndex >= childCount)
            {
                return false;
            }

            VisualElement row = container[hint.FromIndex];
            int toIndex = Math.Clamp(hint.ToIndex, 0, childCount - 1);
            if (toIndex == hint.FromIndex)
            {
                return true; // No-op, but we still claimed the hint.
            }

            container.Insert(toIndex, row); // UI Toolkit auto-removes from old slot first.
            return true;
        }

        private bool IsDescendantOfThisWindow(VisualElement element)
        {
            VisualElement cursor = element;
            while (cursor != null)
            {
                if (cursor == rootVisualElement)
                {
                    return true;
                }
                cursor = cursor.parent;
            }
            return false;
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

            IStepPanelDrawer drawer = ElementDrawerLocator.GetDrawerForValue(
                currentStep.Data, typeof(Step.EntityData)) as IStepPanelDrawer;

            if (drawer == null)
            {
                contentRoot.Add(new Label("(registered drawer for Step.EntityData does not implement IStepPanelDrawer)"));
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
                panelBody);
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
