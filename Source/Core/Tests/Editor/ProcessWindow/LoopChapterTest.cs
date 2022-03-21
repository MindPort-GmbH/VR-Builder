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
    internal class LoopChapterTest : BaseProcessWindowTest
    {
        /// <inheritdoc />
        public override string WhenDescription
        {
            get
            {
                return "User adds a step with two transitions. The first transition points to the same step, the second one is the end of the chapter.";
            }
        }

        /// <inheritdoc />
        public override string ThenDescription
        {
            get
            {
                return "There is a basic process with a loop execution flow.";
            }
        }

        /// <inheritdoc />
        protected override ProcessWindow Given()
        {
            ProcessWindow window = base.Given();

            return window;
        }

        /// <inheritdoc />
        protected override void Then(ProcessWindow window)
        {
            IProcess result = ExtractProcess(window);
            IChapter firstChapter = result.Data.Chapters.First();

            // The chapter exits
            Assert.NotNull(firstChapter);

            // The step exits
            IStep firstStep = firstChapter.Data.FirstStep;
            Assert.NotNull(firstChapter);

            IList<ITransition> transitions = GetTransitionsFromStep(firstStep);

            // It has two transition.
            Assert.That(transitions.Count == 2);

            ITransition firstTransition = transitions[0];
            ITransition secondTransition = transitions[1];
            IStep nextStep;

            // The first step's transition points to itself.
            if (TryToGetStepFromTransition(firstTransition, out nextStep))
            {
                Assert.That(firstStep == nextStep);
            }

            // The second step's transition is the end of the process.
            Assert.False(TryToGetStepFromTransition(secondTransition, out nextStep));
        }
    }
}
