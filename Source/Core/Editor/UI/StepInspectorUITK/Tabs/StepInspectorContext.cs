// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs
{
    internal sealed class StepInspectorContext : IElementDrawerContext
    {
        private readonly object stepDataOwner;
        private readonly Action<object> changeCallback;

        public IStep CurrentStep { get; }
        public IChapter CurrentChapter { get; }
        public IProcess CurrentProcess { get; }

        private StepInspectorContext(
            IStep step,
            IChapter chapter,
            IProcess process,
            object stepDataOwner,
            Action<object> changeCallback)
        {
            CurrentStep = step;
            CurrentChapter = chapter;
            CurrentProcess = process;
            this.stepDataOwner = stepDataOwner;
            this.changeCallback = changeCallback;
        }

        public void NotifyStepModified()
        {
            changeCallback?.Invoke(stepDataOwner);
        }

        /// <summary>
        /// Builds a context around a <see cref="Step.EntityData"/> being drawn.
        /// IStep / IChapter / IProcess come from <c>StepSelectionService</c> once it lands in Phase 4.
        /// </summary>
        public static IElementDrawerContext For(Step.EntityData stepData, Action<object> changeCallback)
        {
            return new StepInspectorContext(
                step: null,
                chapter: null,
                process: null,
                stepDataOwner: stepData,
                changeCallback: changeCallback);
        }
    }
}
