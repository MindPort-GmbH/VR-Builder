// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Windows;

namespace VRBuilder.Core.Editor
{
    /// <summary>
    /// This strategy is used by default and it handles interaction between process assets and various Builder windows.
    /// </summary>
    internal class GraphViewEditingStrategy : IEditingStrategy
    {
        private ProcessEditorWindow processWindow;
        private IStepView stepWindow;

        public IProcess CurrentProcess { get; protected set; }
        public IChapter CurrentChapter { get; protected set; }

        /// <inheritdoc/>
        public void HandleNewProcessWindow(ProcessEditorWindow window)
        {
            processWindow = window;
            processWindow.SetProcess(CurrentProcess);
        }

        /// <inheritdoc/>
        public void HandleNewStepWindow(IStepView window)
        {
            stepWindow = window;
            if (processWindow == null || processWindow.Equals(null))
            {
                HandleCurrentStepChanged(null);
            }
            else
            {
                HandleCurrentStepChanged(processWindow.GetChapter().ChapterMetadata.LastSelectedStep);
            }
        }

        /// <inheritdoc/>
        public void HandleCurrentProcessModified()
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Is also called when the Process Window is open and its "OnDisable" is called e.g. enter play mode, recompile scripts which might be unexpected behavior.
        /// </remarks>
        public void HandleProcessWindowClosed(ProcessEditorWindow window)
        {
            if (processWindow != window)
            {
                return;
            }

            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }
        }

        /// <inheritdoc/>
        public void HandleStepWindowClosed(IStepView window)
        {
            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }

            stepWindow = null;
        }

        /// <inheritdoc/>
        public void HandleStartEditingProcess()
        {
            if (processWindow == null)
            {
                processWindow = EditorWindow.GetWindow<ProcessGraphViewWindow>();
                processWindow.minSize = new Vector2(400f, 100f);
            }
            else
            {
                processWindow.Focus();
            }
        }

        /// <inheritdoc/>
        public void HandleCurrentProcessChanged(string processName)
        {
            if (CurrentProcess != null && CurrentProcess.Data.Name != processName)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }

            EditorPrefs.SetString(GlobalEditorHandler.LastEditedProcessNameKey, processName);
            LoadProcess(ProcessAssetManager.Load(processName));
        }

        private void LoadProcess(IProcess newProcess)
        {
            CurrentProcess = newProcess;
            CurrentChapter = null;

            if (newProcess != null && EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(newProcess.Data, newProcess);
            }

            if (processWindow != null)
            {
                processWindow.SetProcess(CurrentProcess);
                if (stepWindow != null)
                {
                    stepWindow.SetStep(processWindow.GetChapter()?.ChapterMetadata.LastSelectedStep);
                }
            }
            else if (stepWindow != null)
            {
                stepWindow.SetStep(null);
            }
        }

        /// <inheritdoc/>
        public void HandleCurrentStepModified(IStep step)
        {
            processWindow.GetChapter().ChapterMetadata.LastSelectedStep = step;

            if (EditorConfigurator.Instance.Validation.IsAllowedToValidate())
            {
                EditorConfigurator.Instance.Validation.Validate(step.Data, CurrentProcess);
            }

            processWindow.RefreshChapterRepresentation();
        }

        /// <inheritdoc/>
        public void HandleCurrentStepChanged(IStep step)
        {
            StepWindow.ShowInspector();

            if (stepWindow != null)
            {
                if (step != null && EditorConfigurator.Instance.Validation.IsAllowedToValidate())
                {
                    EditorConfigurator.Instance.Validation.Validate(step.Data, CurrentProcess);
                }
                stepWindow.SetStep(step);
            }

            processWindow?.Focus();
        }

        /// <inheritdoc/>
        public void HandleStartEditingStep()
        {
            if (stepWindow == null)
            {
                StepWindow.ShowInspector();
                processWindow?.Focus();
            }
        }

        public void HandleCurrentChapterChanged(IChapter chapter)
        {
            CurrentChapter = chapter;
        }

        /// <inheritdoc/>
        public void HandleProjectIsGoingToUnload()
        {
        }

        /// <inheritdoc/>
        public void HandleProjectIsGoingToSave()
        {
            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }
        }

        /// <inheritdoc/>
        public void HandleExitingPlayMode()
        {
            if (stepWindow != null)
            {
                stepWindow.ResetStepView();
            }
        }

        /// <inheritdoc/>
        public void HandleEnterPlayMode()
        {
        }

        /// <inheritdoc/>
        public void HandleChapterChangeRequest(IChapter chapter)
        {
            processWindow.SetChapter(chapter);
        }
    }
}
