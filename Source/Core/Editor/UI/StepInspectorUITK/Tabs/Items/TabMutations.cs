// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using VRBuilder.Core.Editor.UI.StepInspectorUITK.Windows;
using VRBuilder.Core.Editor.UndoRedo;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Wraps a structural mutation (add / remove / reorder / target-change) in a single
    /// <see cref="ProcessCommand"/>. Notifies the inner editing strategy + the
    /// selection service so the graph view, validation, and every open panel
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
            IStep currentStep = StepSelectionService.CurrentStep;
            if (currentStep != null)
            {
                GlobalEditorHandler.CurrentStepModified(currentStep);
            }
            else
            {
                StepSelectionService.NotifyStepModified();
            }
        }
    }
}
