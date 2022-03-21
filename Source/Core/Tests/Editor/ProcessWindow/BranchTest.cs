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
    internal class BranchTest : BaseProcessWindowTest
    {
        /// <inheritdoc />
        public override string WhenDescription
        {
            get
            {
                return "1. Create three steps.\n" +
                    "2. Connect one of them to the beginning of the chapter.\n" +
                    "3. Add a second transition to that step.\n" +
                    "4. Connect first step to the other two steps.\n";
            }
        }

        /// <inheritdoc />
        public override string ThenDescription
        {
            get
            {
                return "There is a 2-way branched flow.";
            }
        }

        /// <inheritdoc />
        protected override void Then(ProcessWindow window)
        {
            IProcess result = ExtractProcess(window);

            IChapter firstChapter = result.Data.Chapters.First();
            Assert.NotNull(firstChapter);

            IStep firstStep = firstChapter.Data.FirstStep;
            Assert.NotNull(firstStep);

            IList<ITransition> transitions = GetTransitionsFromStep(firstStep);
            Assert.That(transitions.Count == 2);

            foreach (ITransition transition in transitions)
            {
                IStep nextStep;

                if (TryToGetStepFromTransition(transition, out nextStep) == false)
                {
                    Assert.Fail("First step does not always go to another step.");
                }

                IList<ITransition> transitionsFromNextStep = GetTransitionsFromStep(nextStep);
                Assert.That(transitionsFromNextStep.Count == 1);

                IStep endOfChapter;

                if (TryToGetStepFromTransition(transitionsFromNextStep.First(), out endOfChapter))
                {
                    Assert.Fail("Branched step is not the end of the chapter.");
                }
            }
        }
    }
}
