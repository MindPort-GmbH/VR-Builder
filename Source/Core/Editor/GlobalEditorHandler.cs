// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows;
using VRBuilder.Core.Editor.UI.Windows;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Editor
{
    /// <summary>
    /// A class that handles interactions between Builder windows and process assets by using selected <seealso cref="IEditingStrategy"/> strategy.
    /// </summary>
    [InitializeOnLoad]
    public static class GlobalEditorHandler
    {
        public const string LastEditedProcessNameKey = "VRBuilder.Core.Editors.LastEditedProcessName";

        private static IEditingStrategy strategy;

        static GlobalEditorHandler()
        {
            ServiceRegistryLoader.Instance.Register();

            SetDefaultStrategy();

            string lastEditedProcessName = EditorPrefs.GetString(LastEditedProcessNameKey);
            if(!string.IsNullOrEmpty(lastEditedProcessName))
            {
                SetCurrentProcess(lastEditedProcessName);
            }

            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        /// <summary>
        /// Sets the default editing strategy. Reads <see cref="StepInspectorModeState"/> so
        /// a session-only toggle decides between the legacy IMGUI strategy and the UITK one.
        /// Unity launches always start in legacy mode (SessionState is empty).
        /// </summary>
        public static void SetDefaultStrategy()
        {
            SetStrategy(StepInspectorModeState.UseUITK
                ? (IEditingStrategy)new StepInspectorUITKEditingStrategy()
                : new GraphViewEditingStrategy());
        }

        /// <summary>
        /// Sets given <see cref="IEditingStrategy"/> as current strategy.
        /// </summary>
        public static void SetStrategy(IEditingStrategy newStrategy)
        {
            strategy = newStrategy;

            if (newStrategy == null)
            {
                UnityEngine.Debug.LogError("An editing strategy cannot be null, set to default instead.");
                SetDefaultStrategy();
            }
        }

        /// <summary>
        /// Re-installs the strategy matching the current <see cref="StepInspectorModeState"/>
        /// flag and re-seeds it with the currently-edited process and any open process window
        /// so the swap is live without requiring an editor restart.
        /// </summary>
        public static void ApplyStepInspectorMode()
        {
            SetDefaultStrategy();

            // Re-feed any open ProcessEditorWindow so the new strategy knows about it.
            foreach (ProcessEditorWindow window in
                UnityEngine.Resources.FindObjectsOfTypeAll<ProcessGraphViewWindow>())
            {
                if (window != null)
                {
                    strategy.HandleNewProcessWindow(window);
                }
            }

            // Re-seed the current process so the new strategy populates its CurrentProcess /
            // CurrentChapter and any UITK panels receive a selection broadcast.
            string lastProcess = EditorPrefs.GetString(LastEditedProcessNameKey);
            SetCurrentProcess(lastProcess);
        }

        /// <summary>
        /// Returns the current active process, can be null.
        /// </summary>
        public static IProcess GetCurrentProcess()
        {
            return strategy.CurrentProcess;
        }

        /// <summary>
        /// Returns the current active chapter, can be null.
        /// </summary>
        public static IChapter GetCurrentChapter()
        {
            return strategy.CurrentChapter;
        }

        /// <summary>
        /// Returns the step currently being edited, can be null.
        /// </summary>
        public static IStep GetCurrentStep()
        {
            return strategy.CurrentStep;
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a new <see cref="ProcessWindow"/> was just opened.
        /// </summary>
        public static void ProcessWindowOpened(ProcessEditorWindow window)
        {
            strategy.HandleNewProcessWindow(window);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a <see cref="ProcessWindow"/> was closed.
        /// </summary>
        public static void ProcessWindowClosed(ProcessEditorWindow window)
        {
            strategy.HandleProcessWindowClosed(window);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a new step view was just opened.
        /// </summary>
        public static void StepWindowOpened(IStepView window)
        {
            strategy.HandleNewStepWindow(window);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a step view was closed.
        /// </summary>
        public static void StepWindowClosed(IStepView window)
        {
            strategy.HandleStepWindowClosed(window);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when the currently edited process was changed to a different one.
        /// </summary>
        public static void SetCurrentProcess(string processName)
        {
            strategy.HandleCurrentProcessChanged(processName);
        }

        public static void SetCurrentChapter(IChapter chapter)
        {
            strategy.HandleCurrentChapterChanged(chapter);
        }

        public static void RequestNewChapter(IChapter chapter)
        {
            strategy.HandleChapterChangeRequest(chapter);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when user wants to start working on the current process.
        /// </summary>
        public static void StartEditingProcess()
        {
            strategy.HandleStartEditingProcess();
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a designer has just modified the process in the editor.
        /// </summary>
        public static void CurrentProcessModified()
        {
            strategy.HandleCurrentProcessModified();
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when the currently edited <see cref="IStep"/> was modified.
        /// </summary>
        public static void CurrentStepModified(IStep step)
        {
            strategy.HandleCurrentStepModified(step);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a designer chooses a <see cref="IStep"/> to edit.
        /// </summary>
        public static void ChangeCurrentStep(IStep step)
        {
            strategy.HandleCurrentStepChanged(step);
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when a designer wants to start working on a step.
        /// </summary>
        public static void StartEditingStep()
        {
            strategy.HandleStartEditingStep();
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> when the project is going to be unloaded (when assemblies are unloaded, when user starts or stop runtime, when scripts were modified).
        /// </summary>
        public static void ProjectIsGoingToUnload()
        {
            strategy.HandleProjectIsGoingToUnload();
        }

        /// <summary>
        /// Notifies selected <see cref="IEditingStrategy"/> before Unity saves the project (either during the normal exit of the Editor application or when the designer clicks `Save Project`).
        /// </summary>
        public static void ProjectIsGoingToSave()
        {
            strategy.HandleProjectIsGoingToSave();
        }

        public static void EnterPlayMode()
        {
            strategy.HandleEnterPlayMode();
        }

        public static void ExitPlayMode()
        {
            strategy.HandleExitingPlayMode();
        }


        /// <summary>
        /// Sets the current process when a scene is opened.
        /// </summary>
        /// <param name="scene">The opened scene.</param>
        /// <param name="mode">The mode in which the scene was opened.</param>
        /// <remarks>
        /// When having two scenes containing a RuntimeConfigurator, RuntimeConfigurator from the scene which was oped first will be used.
        /// </remarks>
        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            // Test if the scene setup over e.g. RuntimeConfigurationSetup was successful
            if (ServiceRegistry.Has<RuntimeService>() && ServiceRegistry.Get<RuntimeService>().Handler == null)
            {
                SetCurrentProcess(string.Empty);
                return;
            }

            string processPath = ServiceRegistry.Get<RuntimeService>().SelectedProcess;

            if (string.IsNullOrEmpty(processPath))
            {
                SetCurrentProcess(string.Empty);
                return;
            }

            string processName = System.IO.Path.GetFileNameWithoutExtension(processPath);

            IProcess currentProcess = GetCurrentProcess();
            if (currentProcess?.Data?.Name == processName)
            {
                return;
            }

            SetCurrentProcess(processName);
        }
    }
}
