using System;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows
{
    /// <summary>
    /// Single source of truth for "which step is currently being inspected" across all
    /// UITK Step Inspector windows. Fed by <see cref="StepInspectorUITKEditingStrategy"/>;
    /// consumed by the shell window and every detached panel window.
    /// </summary>
    public static class StepSelectionService
    {
        public static event Action<IStep, IChapter, IProcess> SelectionChanged;

        public static IStep CurrentStep { get; private set; }
        public static IChapter CurrentChapter { get; private set; }
        public static IProcess CurrentProcess { get; private set; }

        internal static void Notify(IStep step, IChapter chapter, IProcess process)
        {
            CurrentStep = step;
            CurrentChapter = chapter;
            CurrentProcess = process;
            SelectionChanged?.Invoke(step, chapter, process);
        }

        internal static void NotifyStepModified()
        {
            // Re-broadcast the same selection so listeners can re-bind (the data the step
            // wraps may have changed even if which step is selected has not).
            SelectionChanged?.Invoke(CurrentStep, CurrentChapter, CurrentProcess);
        }
    }
}
