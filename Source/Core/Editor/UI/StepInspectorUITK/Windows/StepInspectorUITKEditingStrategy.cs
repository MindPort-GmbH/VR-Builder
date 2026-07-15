using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Concrete <see cref="IEditingStrategy"/> that pairs the existing GraphView process
    /// editor with the UITK Step Inspector panels. Inherits all graph behaviour from
    /// <see cref="GraphViewEditingStrategy"/> and replaces only the step-inspector hooks.
    /// Unlike the legacy strategy, which owns a single IMGUI <c>StepWindow</c>, UITK mode can
    /// have several panels open at once; each <see cref="DetachedPanelWindow"/> registers as an
    /// <see cref="IStepView"/> and is driven through the standard <see cref="IStepView.SetStep"/>
    /// / <see cref="IStepView.MarkDirty"/> / <see cref="IStepView.ResetStepView"/> contract.
    /// </summary>
    public sealed class StepInspectorUITKEditingStrategy : GraphViewEditingStrategy
    {
        private readonly List<IStepView> stepViews = new List<IStepView>();

        /// <inheritdoc/>
        public override void HandleNewProcessWindow(ProcessEditorWindow window)
        {
            base.HandleNewProcessWindow(window);
            PushSelectionToAllViews();
        }

        /// <inheritdoc/>
        public override void HandleNewStepWindow(IStepView window)
        {
            if (window != null && stepViews.Contains(window) == false)
            {
                stepViews.Add(window);
            }

            window?.SetStep(CurrentSelectedStep());
        }

        /// <inheritdoc/>
        public override void HandleStepWindowClosed(IStepView window)
        {
            stepViews.Remove(window);

            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }
        }

        /// <inheritdoc/>
        public override void HandleCurrentProcessModified()
        {
            foreach (IStepView view in stepViews)
            {
                view.MarkDirty();
            }
        }

        /// <inheritdoc/>
        public override void HandleCurrentStepChanged(IStep step)
        {
            CurrentStep = step;

            if (step != null && EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(step.Data, CurrentProcess);
            }

            EnsureUITKStepPanelOpen();

            foreach (IStepView view in stepViews)
            {
                view.SetStep(step);
            }
        }

        /// <inheritdoc/>
        public override void HandleCurrentStepModified(IStep step)
        {
            CurrentStep = step;
            processWindow.GetChapter().ChapterMetadata.LastSelectedStep = step;

            if (EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(step.Data, CurrentProcess);
            }

            processWindow.RefreshChapterRepresentation();

            // Re-bind (not full rebuild) so the drag-reorder fast-path in DetachedPanelWindow.MarkDirty stays live.
            foreach (IStepView view in stepViews)
            {
                view.MarkDirty();
            }
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
            PushSelectionToAllViews();
        }

        /// <inheritdoc/>
        public override void HandleCurrentChapterChanged(IChapter chapter)
        {
            base.HandleCurrentChapterChanged(chapter);
            PushSelectionToAllViews();
        }

        /// <inheritdoc/>
        public override void HandleChapterChangeRequest(IChapter chapter)
        {
            base.HandleChapterChangeRequest(chapter);
            PushSelectionToAllViews();
        }

        /// <inheritdoc/>
        public override void HandleExitingPlayMode()
        {
            foreach (IStepView view in stepViews)
            {
                view.ResetStepView();
            }
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

        private IStep CurrentSelectedStep()
        {
            return CurrentChapter?.ChapterMetadata?.LastSelectedStep;
        }

        private void PushSelectionToAllViews()
        {
            IStep step = CurrentSelectedStep();
            CurrentStep = step;
            foreach (IStepView view in stepViews)
            {
                view.SetStep(step);
            }
        }
    }
}
