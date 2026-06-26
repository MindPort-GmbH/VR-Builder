using System;
using VRBuilder.Core.Editor.UndoRedo;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Wraps a structural mutation (add / remove / reorder / target-change) in a single
    /// <see cref="ProcessCommand"/>. Notifies the editing strategy through
    /// <see cref="GlobalEditorHandler"/> so the graph view, validation, and every open panel
    /// rebuild themselves.
    /// </summary>
    internal static class TabMutations
    {
        public static void Do(Action doAction, Action undoAction)
        {
            RevertableChangesHandler.Do(new ProcessCommand(
                () =>
                {
                    doAction();
                    NotifyChanged();
                },
                () =>
                {
                    undoAction();
                    NotifyChanged();
                }));
        }

        private static void NotifyChanged()
        {
            IStep currentStep = GlobalEditorHandler.GetCurrentStep();
            if (currentStep != null)
            {
                GlobalEditorHandler.CurrentStepModified(currentStep);
            }
            else
            {
                GlobalEditorHandler.CurrentProcessModified();
            }
        }
    }
}
