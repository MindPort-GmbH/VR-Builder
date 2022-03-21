// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using VRBuilder.Core;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.Tests.ProcessWindowTests
{
    internal class AddTwoStepsRemoveOneTest : BaseProcessWindowTest
    {
        /// <inheritdoc />
        public override string WhenDescription
        {
            get
            {
                return "1. Add two new steps." + "\n" +
                       "2. Delete one step." + "\n" +
                       "3. Connect the step to the start of the chapter.";
            }
        }

        /// <inheritdoc />
        public override string ThenDescription
        {
            get
            {
                return "There is a process with exactly one step created.";
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
            Assert.That(transitions.Count == 1);

            IStep nextStep;

            if (TryToGetStepFromTransition(transitions.First(), out nextStep))
            {
                Assert.Fail("First step is not the end of the chapter.");
            }

            Assert.Null(nextStep);
        }
    }
}
