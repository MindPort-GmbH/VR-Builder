using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Editor;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;
using VRBuilder.Core.Editor.UI.Windows;

namespace MindPort.VRBuilderProT.StepInspectorPureUITK
{
    [InitializeOnLoad]
    internal static class StepInspectorPureUITKBootstrap
    {
        static StepInspectorPureUITKBootstrap()
        {
            GlobalEditorHandler.SetStrategy(new StepInspectorPureUITKEditingStrategy());

            string lastEditedProcess = EditorPrefs.GetString(GlobalEditorHandler.LastEditedProcessNameKey);
            GlobalEditorHandler.SetCurrentProcess(lastEditedProcess);

            CloseLegacyStepWindows();
        }

        private static void CloseLegacyStepWindows()
        {
            foreach (StepWindow window in Resources.FindObjectsOfTypeAll<StepWindow>())
            {
                if (window != null)
                {
                    window.Close();
                }
            }
        }
    }

    internal sealed class StepInspectorPureUITKEditingStrategy : IEditingStrategy
    {
        private ProcessEditorWindow processWindow;
        private IStepView legacyStepWindow;

        internal static bool AllowLegacyStepWindow { get; set; }

        public IProcess CurrentProcess { get; private set; }
        public IChapter CurrentChapter { get; private set; }

        public void HandleNewProcessWindow(ProcessEditorWindow window)
        {
            processWindow = window;
            TrySetProcess(processWindow, CurrentProcess);
            CurrentChapter = GetProcessWindowChapter(processWindow) ?? GlobalEditorHandler.GetCurrentChapter();
            StepInspectorPureUITKWindow.BindOpenWindows(GetSelectedStep(), CurrentChapter, CurrentProcess);
            SyncLegacyStep(GetSelectedStep());
        }

        public void HandleNewStepWindow(IStepView window)
        {
            if (AllowLegacyStepWindow)
            {
                AllowLegacyStepWindow = false;
                legacyStepWindow = window;

                IStep selectedStep = GetSelectedStep();
                if (selectedStep != null)
                {
                    window.SetStep(selectedStep);
                }

                return;
            }

            if (window is EditorWindow legacyWindow)
            {
                legacyWindow.Close();
            }

            IChapter chapter = GetActiveChapter();
            IProcess process = CurrentProcess ?? GlobalEditorHandler.GetCurrentProcess();
            IStep step = chapter?.ChapterMetadata?.LastSelectedStep;

            if (step != null)
            {
                StepInspectorPureUITKWindow.OpenOrFocusAndBind(step, chapter, process, false);
            }
            else
            {
                StepInspectorPureUITKWindow.BindOpenWindows(null, chapter, process);
            }
        }

        public void HandleCurrentProcessModified()
        {
            TryRefreshChapterRepresentation(processWindow);
            StepInspectorPureUITKWindow.RequestExternalRefresh();
            SyncLegacyDirty();
        }

        public void HandleProcessWindowClosed(ProcessEditorWindow window)
        {
            if (!ReferenceEquals(processWindow, window))
            {
                return;
            }

            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }
        }

        public void HandleStepWindowClosed(IStepView window)
        {
            if (ReferenceEquals(legacyStepWindow, window))
            {
                legacyStepWindow = null;
            }

            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }
        }

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

        public void HandleCurrentProcessChanged(string processName)
        {
            if (CurrentProcess != null && CurrentProcess.Data.Name != processName)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }

            EditorPrefs.SetString(GlobalEditorHandler.LastEditedProcessNameKey, processName);
            LoadProcess(ProcessAssetManager.Load(processName));
        }

        public void HandleCurrentStepModified(IStep step)
        {
            IChapter chapter = GetActiveChapter();
            if (chapter?.ChapterMetadata != null)
            {
                chapter.ChapterMetadata.LastSelectedStep = step;
                CurrentChapter = chapter;
            }

            TryRefreshChapterRepresentation(processWindow);
            StepInspectorPureUITKWindow.BindOpenWindows(step, CurrentChapter, CurrentProcess);
            StepInspectorPureUITKWindow.RequestExternalRefresh();
            SyncLegacyDirty();
        }

        public void HandleStartEditingStep()
        {
            CurrentChapter = GetActiveChapter();
            StepInspectorPureUITKWindow.OpenOrFocusAndBind(GetSelectedStep(), CurrentChapter, CurrentProcess, false);
            processWindow?.Focus();
        }

        public void HandleCurrentStepChanged(IStep step)
        {
            CurrentChapter = GetActiveChapter();

            if (CurrentChapter?.ChapterMetadata != null)
            {
                CurrentChapter.ChapterMetadata.LastSelectedStep = step;
            }

            if (step != null)
            {
                StepInspectorPureUITKWindow.OpenOrFocusAndBind(step, CurrentChapter, CurrentProcess, false);
            }
            else
            {
                StepInspectorPureUITKWindow.BindOpenWindows(null, CurrentChapter, CurrentProcess);
            }

            SyncLegacyStep(step);
            processWindow?.Focus();
        }

        public void HandleCurrentChapterChanged(IChapter chapter)
        {
            CurrentChapter = chapter;
            StepInspectorPureUITKWindow.BindOpenWindows(GetSelectedStep(), CurrentChapter, CurrentProcess);
        }

        public void HandleChapterChangeRequest(IChapter chapter)
        {
            TrySetChapter(processWindow, chapter);
            CurrentChapter = chapter;
            StepInspectorPureUITKWindow.BindOpenWindows(GetSelectedStep(), CurrentChapter, CurrentProcess);
        }

        public void HandleProjectIsGoingToUnload()
        {
        }

        public void HandleProjectIsGoingToSave()
        {
            if (CurrentProcess != null)
            {
                ProcessAssetManager.Save(CurrentProcess);
            }
        }

        public void HandleExitingPlayMode()
        {
            StepInspectorPureUITKWindow.RequestExternalRefresh();

            if (legacyStepWindow != null)
            {
                legacyStepWindow.ResetStepView();
            }
        }

        public void HandleEnterPlayMode()
        {
        }

        private void LoadProcess(IProcess newProcess)
        {
            CurrentProcess = newProcess;
            CurrentChapter = null;

            if (processWindow != null)
            {
                TrySetProcess(processWindow, CurrentProcess);
                CurrentChapter = GetProcessWindowChapter(processWindow) ?? GlobalEditorHandler.GetCurrentChapter();
            }
            else
            {
                CurrentChapter = GlobalEditorHandler.GetCurrentChapter();
            }

            StepInspectorPureUITKWindow.BindOpenWindows(GetSelectedStep(), CurrentChapter, CurrentProcess);
            StepInspectorPureUITKWindow.RequestExternalRefresh();
            SyncLegacyStep(GetSelectedStep());
        }

        private void SyncLegacyStep(IStep step)
        {
            if (legacyStepWindow == null)
            {
                return;
            }

            if (legacyStepWindow is EditorWindow ew && ew == null)
            {
                legacyStepWindow = null;
                return;
            }

            legacyStepWindow.SetStep(step);
        }

        private void SyncLegacyDirty()
        {
            if (legacyStepWindow == null)
            {
                return;
            }

            if (legacyStepWindow is EditorWindow ew && ew == null)
            {
                legacyStepWindow = null;
                return;
            }

            legacyStepWindow.MarkDirty();
        }

        private IChapter GetActiveChapter()
        {
            return GetProcessWindowChapter(processWindow) ?? CurrentChapter ?? GlobalEditorHandler.GetCurrentChapter();
        }

        private IStep GetSelectedStep()
        {
            return CurrentChapter?.ChapterMetadata?.LastSelectedStep;
        }

        private static void TrySetProcess(ProcessEditorWindow window, IProcess process)
        {
            InvokeInstanceMethod(window, "SetProcess", process);
        }

        private static IChapter GetProcessWindowChapter(ProcessEditorWindow window)
        {
            object chapter = InvokeInstanceMethod(window, "GetChapter");
            return chapter as IChapter;
        }

        private static void TrySetChapter(ProcessEditorWindow window, IChapter chapter)
        {
            InvokeInstanceMethod(window, "SetChapter", chapter);
        }

        private static void TryRefreshChapterRepresentation(ProcessEditorWindow window)
        {
            InvokeInstanceMethod(window, "RefreshChapterRepresentation");
        }

        private static object InvokeInstanceMethod(object target, string methodName, params object[] args)
        {
            if (target == null)
            {
                return null;
            }

            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
            {
                return null;
            }

            return method.Invoke(target, args);
        }
    }
}