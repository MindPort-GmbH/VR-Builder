using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.DragDrop;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Editor window that hosts one Step Inspector panel. Every panel opens in its own
    /// instance so users can dock them via Unity's native dock system however they want.
    /// </summary>
    public class DetachedPanelWindow : EditorWindow, IStepView, IHasCustomMenu
    {
        [SerializeField] private string panelId;
        [SerializeField] private bool isLocked;
        [SerializeField] private string lockedStepGuid;

        private static GUIStyle lockButtonStyle;
        // Floor below which the panel layout starts to break (labels/inputs overlap, buttons clip).
        // Applied to every instance in OnEnable so it also covers windows spawned arg-less by Unity's
        // native "Add Tab" menu, which never run through the static factory methods below.
        private static readonly Vector2 MinWindowSize = new Vector2(420f, 320f);
        private VisualElement contentRoot;
        private StepPanelBuildContext panelBuildContext;
        // The step this panel currently renders. Not serialized: restored from lockedStepGuid
        // after domain reload when the panel is locked.
        private IStep step;
        private ISceneObjectRegistry subscribedSceneObjectRegistry;
        private bool sceneChangeRebuildScheduled;
        public string PanelId => panelId;
        public bool IsLocked => isLocked;
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
            window.titleContent = new GUIContent(TitleFor(panelId), IconFor(panelId));
            window.minSize = MinWindowSize;
            // No Show() here — caller docks via AddTab.
            return window;
        }
        private static DetachedPanelWindow Create(string panelId)
        {
            DetachedPanelWindow window = CreateInstance<DetachedPanelWindow>();
            window.panelId = panelId;
            window.titleContent = new GUIContent(TitleFor(panelId), IconFor(panelId));
            window.minSize = MinWindowSize;
            window.Show();
            window.Focus();
            return window;
        }
        private void OnEnable()
        {
            EnsurePanelId();
            minSize = MinWindowSize;
            TryRestoreLockedStepFromGuid();
            // Register through the same channel the legacy StepWindow uses; the active strategy
            // pushes the current step back via SetStep.
            GlobalEditorHandler.StepWindowOpened(this);
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.hierarchyChanged += OnSceneObjectsChanged;
            EditorApplication.projectChanged += OnSceneObjectsChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SubscribeToSceneObjectChanges();
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.hierarchyChanged -= OnSceneObjectsChanged;
            EditorApplication.projectChanged -= OnSceneObjectsChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.delayCall -= ExecuteSceneChangeRebuild;
            sceneChangeRebuildScheduled = false;
            UnsubscribeFromSceneObjectChanges();
        }
        private void OnDestroy()
        {
            panelBuildContext?.Dispose();
            panelBuildContext = null;
            GlobalEditorHandler.StepWindowClosed(this);
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
            UpdateWindowTitle();
            SubscribeToSceneObjectChanges();
            Rebuild();
        }
        /// <inheritdoc/>
        public void SetStep(IStep newStep)
        {
            if (isLocked)
            {
                return;
            }
            step = newStep;
            RunValidationIfAllowed();
            Rebuild();
        }
        /// <inheritdoc/>
        public void MarkDirty()
        {
            // Re-bind the current step after a data mutation (graph edit, add/remove, reorder).
            // Defensive: never tear down the visual tree while a drag is in flight. Today
            // nothing marks dirty mid-drag, but a future decorator could — and rebuilding while
            // DragDropBinder holds references to row elements would corrupt pointer capture.
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
            // Structural changes invalidate the previous validation report; re-run before
            // Rebuild so DecorateMember / DecorateEntityHeader read fresh entries.
            RunValidationIfAllowed();
            Rebuild();
        }
        /// <inheritdoc/>
        public void ResetStepView()
        {
            isLocked = false;
            lockedStepGuid = null;
            step = null;
            Rebuild();
        }

        /// <summary>
        /// Adds Lock to Unity's native pane options menu (the same ⋮ menu as Add Tab).
        /// </summary>
        public void AddItemsToMenu(GenericMenu menu)
        {
            bool canLock = step?.Data != null || isLocked;
            if (canLock)
            {
                menu.AddItem(new GUIContent("Lock"), isLocked, () => SetLocked(!isLocked));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Lock"));
            }
        }

        /// <summary>
        /// Draws the lock icon in Unity's native tab header, immediately left of the pane ⋮ menu.
        /// </summary>
        protected virtual void ShowButton(Rect buttonRect)
        {
            bool canLock = step?.Data != null || isLocked;
            EditorGUI.BeginDisabledGroup(canLock == false);
            EditorGUI.BeginChangeCheck();
            bool newLocked = GUI.Toggle(buttonRect, isLocked, GUIContent.none, LockButtonStyle);
            if (EditorGUI.EndChangeCheck() && canLock)
            {
                SetLocked(newLocked);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void SetLocked(bool locked)
        {
            if (locked == isLocked)
            {
                return;
            }

            if (locked && step?.Data == null)
            {
                return;
            }

            isLocked = locked;
            if (isLocked)
            {
                UpdateLockedStepGuid();
            }
            else
            {
                lockedStepGuid = null;
                step = ResolveCurrentSelectedStep();
            }

            UpdateWindowTitle();
            Rebuild();
            Repaint();
        }

        private static GUIStyle LockButtonStyle
        {
            get
            {
                if (lockButtonStyle == null)
                {
                    lockButtonStyle = GUI.skin.GetStyle("IN LockButton");
                }

                return lockButtonStyle;
            }
        }

        private void UpdateLockedStepGuid()
        {
            if (step?.StepMetadata != null)
            {
                lockedStepGuid = step.StepMetadata.Guid.ToString();
            }
            else
            {
                lockedStepGuid = null;
            }
        }
        private void TryRestoreLockedStepFromGuid()
        {
            if (isLocked == false || step != null || string.IsNullOrEmpty(lockedStepGuid))
            {
                return;
            }
            if (Guid.TryParse(lockedStepGuid, out Guid guid) == false)
            {
                return;
            }
            step = FindStepByGuid(guid);
        }
        private static IStep FindStepByGuid(Guid guid)
        {
            IProcess process = GlobalEditorHandler.GetCurrentProcess();
            if (process?.Data?.Chapters == null)
            {
                return null;
            }
            foreach (IChapter chapter in process.Data.Chapters)
            {
                if (chapter?.Data?.Steps == null)
                {
                    continue;
                }
                foreach (IStep candidate in chapter.Data.Steps)
                {
                    if (candidate?.StepMetadata?.Guid == guid)
                    {
                        return candidate;
                    }
                }
            }
            return null;
        }
        private static IStep ResolveCurrentSelectedStep()
        {
            // LastSelectedStep is only written on step modification, not graph selection.
            // CurrentStep on the editing strategy is updated every time the user picks a node.
            return GlobalEditorHandler.GetCurrentStep();
        }
        private bool TryGetBoundEntityData(out Step.EntityData entityData)
        {
            entityData = null;
            if (step?.Data == null)
            {
                return false;
            }
            entityData = step.Data as Step.EntityData;
            return entityData != null;
        }
        private StepPanelBuildContext GetPanelBuildContext()
        {
            if (panelBuildContext == null)
            {
                panelBuildContext = CreatePanelBuildContextForWindow();
            }
            return panelBuildContext;
        }
        private static StepPanelBuildContext CreatePanelBuildContextForWindow()
        {
            IElementDrawer registered = ElementDrawerLocator.GetDrawerForValue(null, typeof(Step.EntityData));
            if (registered != null)
            {
                Type drawerType = registered.GetType();
                if (Activator.CreateInstance(drawerType) is StepElementDrawer drawer)
                {
                    return drawer.CreatePanelBuildContext();
                }
            }
            return new StepElementDrawer().CreatePanelBuildContext();
        }
        private string BuildHeaderTitle()
        {
            string baseTitle = TitleFor(panelId);
            if (isLocked == false || step?.Data == null)
            {
                return baseTitle;
            }
            string stepName = step.Data.Name;
            return string.IsNullOrEmpty(stepName) ? baseTitle : $"{baseTitle} — {stepName}";
        }
        private void UpdateWindowTitle()
        {
            titleContent = new GUIContent(BuildHeaderTitle(), IconFor(panelId));
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
            VisualElement[] rowChildren = DropTargetRegistry.CollectRowElements(container);
            if (hint.FromIndex < 0 || hint.FromIndex >= rowChildren.Length)
            {
                return false;
            }

            VisualElement row = rowChildren[hint.FromIndex];
            int toIndex = Math.Clamp(hint.ToIndex, 0, rowChildren.Length);
            if (toIndex == hint.FromIndex)
            {
                return true; // No-op, but we still claimed the hint.
            }

            // Insert among row siblings only — containers may also hold empty hints or the
            // transient insertion line from the drag preview.
            int domInsertIndex = container.childCount;
            int rowIndex = 0;
            for (int i = 0; i < container.childCount; i++)
            {
                VisualElement child = container[i];
                if (!DropTargetRegistry.IsRowElement(child))
                {
                    continue;
                }

                if (rowIndex == toIndex)
                {
                    domInsertIndex = i;
                    break;
                }

                rowIndex++;
                domInsertIndex = i + 1;
            }

            container.Insert(domInsertIndex, row); // UI Toolkit auto-removes from old slot first.
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
        /// <summary>
        /// Mirrors the legacy <see cref="UI.Windows.StepWindow"/> subscriptions so scene-side
        /// edits (PSO deletion, group membership, registry refresh) refresh retained UITK UI.
        /// Coalesced via <see cref="ScheduleSceneChangeRebuild"/> because hierarchy changes
        /// can fire many times per operation.
        /// </summary>
        private void OnSceneObjectsChanged()
        {
            ScheduleSceneChangeRebuild();
        }
        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            ScheduleSceneChangeRebuild();
        }
        private void SubscribeToSceneObjectChanges()
        {
            UnsubscribeFromSceneObjectChanges();
            if (RuntimeConfigurator.Exists == false)
            {
                return;
            }
            subscribedSceneObjectRegistry = RuntimeConfigurator.Configuration?.SceneObjectRegistry;
            if (subscribedSceneObjectRegistry != null)
            {
                subscribedSceneObjectRegistry.Changed += OnSceneObjectsChanged;
            }
            if (SceneObjectGroups.Instance != null)
            {
                SceneObjectGroups.Instance.Changed += OnSceneObjectsChanged;
            }
        }
        private void UnsubscribeFromSceneObjectChanges()
        {
            if (subscribedSceneObjectRegistry != null)
            {
                subscribedSceneObjectRegistry.Changed -= OnSceneObjectsChanged;
                subscribedSceneObjectRegistry = null;
            }
            if (SceneObjectGroups.Instance != null)
            {
                SceneObjectGroups.Instance.Changed -= OnSceneObjectsChanged;
            }
        }
        private void ScheduleSceneChangeRebuild()
        {
            if (contentRoot == null)
            {
                return;
            }
            if (DragSession.IsActive)
            {
                return;
            }
            if (sceneChangeRebuildScheduled)
            {
                return;
            }
            sceneChangeRebuildScheduled = true;
            EditorApplication.delayCall += ExecuteSceneChangeRebuild;
        }
        private void ExecuteSceneChangeRebuild()
        {
            EditorApplication.delayCall -= ExecuteSceneChangeRebuild;
            sceneChangeRebuildScheduled = false;
            if (this == null)
            {
                return;
            }
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
        private void RunValidationIfAllowed()
        {
            if (step?.Data == null) return;
            IProcess process = GlobalEditorHandler.GetCurrentProcess();
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
            UpdateWindowTitle();
            contentRoot.Clear();
            VisualElement body = BuildPanelBody();
            PanelHost host = new PanelHost(
                panelId,
                BuildHeaderTitle(),
                body,
                IconFor(panelId));
            host.style.flexGrow = 1f;
            if (TryGetBoundEntityData(out Step.EntityData stepData))
            {
                AppendValidationIcon(host, stepData, panelId);
            }
            contentRoot.Add(host);
        }
        private VisualElement BuildPanelBody()
        {
            if (isLocked && TryGetBoundEntityData(out Step.EntityData entityData) == false)
            {
                if (string.IsNullOrEmpty(lockedStepGuid) == false)
                {
                    return BuildCenteredMessage(Tooltips.PanelLockUnavailable);
                }
                return BuildCenteredMessage("Select a step in the Process Editor.");
            }
            if (TryGetBoundEntityData(out entityData) == false)
            {
                return BuildCenteredMessage("Select a step in the Process Editor.");
            }
            VisualElement panelBody = GetPanelBuildContext().BuildPanel(panelId, entityData);
            if (panelBody == null)
            {
                return new Label($"(no content for panel '{panelId}')");
            }
            return panelBody;
        }
        private static VisualElement BuildCenteredMessage(string message)
        {
            Label label = new Label(message);
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.flexGrow = 1f;
            label.AddToClassList("vrb-panel-stub");
            return label;
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
        // Sibling of TitleFor: the tab/header icon for a panel. Loads the single "_light" art from the
        // package's static icons folder (used on both skins), or null when no PNG is present (tab → text-only).
        private static Texture2D IconFor(string id)
        {
            switch (id)
            {
                case PanelIds.Header:      return LoadIcon("icon_step_light");
                case PanelIds.Behaviors:   return LoadIcon("icon_behaviors_light");
                case PanelIds.Transitions: return LoadIcon("icon_transitions_light");
                case PanelIds.Unlocked:    return LoadIcon("icon_unlocked-objects_light");
                default: return null;
            }
        }
        private static Texture2D LoadIcon(string fileStem)
        {
            const string packageDir = "Packages/co.mindport.vrbuilder.core/Source/Core/StaticAssets/Icons/";
            // Used when the package is mounted via file: protocol or relocated under Assets/.
            const string fallbackDir = "Assets/MindPort/VR Builder/Core/Source/Core/StaticAssets/Icons/";
            return AssetDatabase.LoadAssetAtPath<Texture2D>(packageDir + fileStem + ".png")
                ?? AssetDatabase.LoadAssetAtPath<Texture2D>(fallbackDir + fileStem + ".png");
        }
    }
}
