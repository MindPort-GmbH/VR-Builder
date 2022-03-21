// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VRBuilder.Core;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.Tests.ProcessWindowTests
{
    internal class LinearChapterTest : BaseProcessWindowTest
    {
        private int iteratedSteps;
        private List<IStep> validatedSteps = new List<IStep>();

        /// <inheritdoc />
        public override string WhenDescription
        {
            get
            {
                return "Add a step to the chapter. Connect it to the chapter's start node.\n" +
                    "Add a second chapter.\n" +
                    "Add a step to that chapter and connect it to the chapter's start node.";
            }
        }

        /// <inheritdoc />
        public override string ThenDescription
        {
            get
            {
                return "There is a process with exactly one execution flow.";
            }
        }

        /// <inheritdoc />
        protected override ProcessWindow Given()
        {
            ProcessWindow window = base.Given();
            iteratedSteps = 0;

            return window;
        }

        /// <inheritdoc />
        protected override void Then(ProcessWindow window)
        {
            IProcess result = ExtractProcess(window);
            IChapter firstChapter = result.Data.Chapters.First();

            Assert.NotNull(firstChapter);

            IStep firstStep = firstChapter.Data.FirstStep;
            IList<IStep> steps = firstChapter.Data.Steps;

            validatedSteps.Clear();
            ValidateLinearProcess(firstStep);

            Assert.That(iteratedSteps == steps.Count);
        }

        private void ValidateLinearProcess(IStep step)
        {
            iteratedSteps++;

            // The step exits
            Assert.NotNull(step);

            // The step was not validated before (In case of circular flows).
            Assert.IsFalse(validatedSteps.Contains(step));

            IList<ITransition> transitions = GetTransitionsFromStep(step);

            // It has only one transition.
            Assert.That(transitions.Count == 1);
            validatedSteps.Add(step);
            IStep nextStep;

            // In case the transition points to another step, the process starts again.
            // If not, the step is the end of the chapter.
            if (TryToGetStepFromTransition(transitions.First(), out nextStep))
            {
                ValidateLinearProcess(nextStep);
            }
        }
    }
}
