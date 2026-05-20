using System.Reflection;
using VRBuilder.Core.Editor.UI.GraphView;
using VRBuilder.Core.Editor.UI.GraphView.Windows;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Wraps the currently-installed <see cref="IEditingStrategy"/> so the legacy IMGUI
    /// flow keeps working while the UITK windows mirror selection through
    /// <see cref="StepSelectionService"/>. Installed on first UITK window open and
    /// uninstalled when the last UITK window closes.
    /// </summary>
    internal sealed class StepInspectorUITKEditingStrategy : IEditingStrategy
    {
        private static StepInspectorUITKEditingStrategy active;
        private static int referenceCount;

        private readonly IEditingStrategy inner;

        private StepInspectorUITKEditingStrategy(IEditingStrategy inner)
        {
            this.inner = inner;
        }

        public IProcess CurrentProcess => inner.CurrentProcess;
        public IChapter CurrentChapter => inner.CurrentChapter;

        public void HandleNewProcessWindow(ProcessEditorWindow window)
        {
            inner.HandleNewProcessWindow(window);
            RebroadcastFromCurrentSelection();
        }

        public void HandleNewStepWindow(IStepView window) => inner.HandleNewStepWindow(window);

        public void HandleCurrentProcessModified()
        {
            inner.HandleCurrentProcessModified();
            StepSelectionService.NotifyStepModified();
        }

        public void HandleProcessWindowClosed(ProcessEditorWindow window) => inner.HandleProcessWindowClosed(window);
        public void HandleStepWindowClosed(IStepView window) => inner.HandleStepWindowClosed(window);
        public void HandleStartEditingProcess() => inner.HandleStartEditingProcess();

        public void HandleCurrentProcessChanged(string processName)
        {
            inner.HandleCurrentProcessChanged(processName);
            RebroadcastFromCurrentSelection();
        }

        public void HandleCurrentStepModified(IStep step)
        {
            inner.HandleCurrentStepModified(step);
            StepSelectionService.Notify(step, inner.CurrentChapter, inner.CurrentProcess);
        }

        public void HandleStartEditingStep() => inner.HandleStartEditingStep();

        public void HandleCurrentStepChanged(IStep step)
        {
            inner.HandleCurrentStepChanged(step);
            StepSelectionService.Notify(step, inner.CurrentChapter, inner.CurrentProcess);
        }

        public void HandleCurrentChapterChanged(IChapter chapter)
        {
            inner.HandleCurrentChapterChanged(chapter);
            RebroadcastFromCurrentSelection();
        }

        public void HandleChapterChangeRequest(IChapter chapter)
        {
            inner.HandleChapterChangeRequest(chapter);
            RebroadcastFromCurrentSelection();
        }

        public void HandleProjectIsGoingToUnload() => inner.HandleProjectIsGoingToUnload();
        public void HandleProjectIsGoingToSave() => inner.HandleProjectIsGoingToSave();
        public void HandleExitingPlayMode() => inner.HandleExitingPlayMode();
        public void HandleEnterPlayMode() => inner.HandleEnterPlayMode();

        private void RebroadcastFromCurrentSelection()
        {
            IChapter chapter = inner.CurrentChapter;
            IStep step = chapter?.ChapterMetadata?.LastSelectedStep;
            StepSelectionService.Notify(step, chapter, inner.CurrentProcess);
        }

        /// <summary>
        /// Adds one window to the activation count. Installs the decorator on the first call.
        /// </summary>
        public static void Acquire()
        {
            referenceCount++;
            if (active != null)
            {
                return;
            }

            IEditingStrategy currentStrategy = ReadCurrentStrategy();
            if (currentStrategy == null || currentStrategy is StepInspectorUITKEditingStrategy)
            {
                return;
            }

            active = new StepInspectorUITKEditingStrategy(currentStrategy);
            GlobalEditorHandler.SetStrategy(active);

            // Seed StepSelectionService from whatever the inner strategy currently exposes
            // so a window opened after a step is already selected still shows the right step.
            active.RebroadcastFromCurrentSelection();
        }

        /// <summary>
        /// Removes one window from the activation count. Uninstalls the decorator and restores
        /// the original strategy when the count hits zero.
        /// </summary>
        public static void Release()
        {
            if (referenceCount > 0)
            {
                referenceCount--;
            }

            if (referenceCount > 0 || active == null)
            {
                return;
            }

            GlobalEditorHandler.SetStrategy(active.inner);
            active = null;
        }

        private static IEditingStrategy ReadCurrentStrategy()
        {
            FieldInfo field = typeof(GlobalEditorHandler).GetField(
                "strategy",
                BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null) as IEditingStrategy;
        }
    }
}
