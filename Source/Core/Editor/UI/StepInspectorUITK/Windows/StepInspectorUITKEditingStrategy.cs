using UnityEngine;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Concrete <see cref="IEditingStrategy"/> that pairs the existing GraphView process
    /// editor with the UITK Step Inspector panels. Inherits all graph behaviour from
    /// <see cref="GraphViewEditingStrategy"/> and replaces only the step-inspector hooks:
    /// instead of opening/updating the legacy IMGUI <c>StepWindow</c>, it broadcasts
    /// through <see cref="StepSelectionService"/> and auto-opens the UITK Step panel.
    /// </summary>
    public sealed class StepInspectorUITKEditingStrategy : GraphViewEditingStrategy
    {
        /// <inheritdoc/>
        public override void HandleNewProcessWindow(ProcessEditorWindow window)
        {
            base.HandleNewProcessWindow(window);
            RebroadcastFromCurrentSelection();
        }

        /// <inheritdoc/>
        public override void HandleNewStepWindow(IStepView window)
        {
            // UITK mode does not own the legacy StepWindow. If the user opens one
            // manually, leave it inert — it will keep showing its last state.
        }

        /// <inheritdoc/>
        public override void HandleStepWindowClosed(IStepView window)
        {
            // No legacy-window ownership in UITK mode.
        }

        /// <inheritdoc/>
        public override void HandleCurrentProcessModified()
        {
            // UITK panels rebuild via StepSelectionService instead of stepWindow.MarkDirty().
            StepSelectionService.NotifyStepModified();
        }

        /// <inheritdoc/>
        public override void HandleCurrentStepChanged(IStep step)
        {
            if (step != null && EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(step.Data, CurrentProcess);
            }

            EnsureUITKStepPanelOpen();
            StepSelectionService.Notify(step, CurrentChapter, CurrentProcess);
        }

        /// <inheritdoc/>
        public override void HandleCurrentStepModified(IStep step)
        {
            processWindow.GetChapter().ChapterMetadata.LastSelectedStep = step;

            if (EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(step.Data, CurrentProcess);
            }

            processWindow.RefreshChapterRepresentation();
            StepSelectionService.Notify(step, CurrentChapter, CurrentProcess);
        }

        /// <inheritdoc/>
        public override void HandleStartEditingStep()
        {
            EnsureUITKStepPanelOpen();
        }

        /// <inheritdoc/>
        public override void HandleCurrentProcessChanged(string processName)
        {
            base.HandleCurrentProcessChanged(processName);
            RebroadcastFromCurrentSelection();
        }

        /// <inheritdoc/>
        public override void HandleCurrentChapterChanged(IChapter chapter)
        {
            base.HandleCurrentChapterChanged(chapter);
            RebroadcastFromCurrentSelection();
        }

        /// <inheritdoc/>
        public override void HandleChapterChangeRequest(IChapter chapter)
        {
            base.HandleChapterChangeRequest(chapter);
            RebroadcastFromCurrentSelection();
        }

        /// <inheritdoc/>
        public override void HandleExitingPlayMode()
        {
            // No legacy stepWindow.ResetStepView() — UITK panels listen to selection
            // changes and rebuild themselves.
        }

        private static void EnsureUITKStepPanelOpen()
        {
            foreach (DetachedPanelWindow existing in Resources.FindObjectsOfTypeAll<DetachedPanelWindow>())
            {
                if (existing != null)
                {
                    return;
                }
            }

            // Mirror the "Open All" menu layout: Step anchor on top, Behaviors / Transitions /
            // Unlocked docked as tabs below. Matches what the user gets by running the menu
            // item manually, so auto-open and explicit-open produce the same window setup.
            StepInspectorMenu.OpenAll();
        }

        private void RebroadcastFromCurrentSelection()
        {
            IStep step = CurrentChapter?.ChapterMetadata?.LastSelectedStep;
            StepSelectionService.Notify(step, CurrentChapter, CurrentProcess);
        }
    }
}
