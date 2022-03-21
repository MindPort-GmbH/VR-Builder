// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using System.Linq;
using NUnit.Framework;
using VRBuilder.Core;
using System.Collections.Generic;
using VRBuilder.Editor.UI.Windows;

namespace VRBuilder.Editor.Tests.ProcessWindowTests
{
    internal class AddOneStepTest : BaseProcessWindowTest
    {
        /// <inheritdoc />
        public override string WhenDescription => "Add one step to the workflow. Connect it to the start of the chapter.";

        /// <inheritdoc />
        public override string ThenDescription => "There is a process with exactly one step created.";

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

            if (TryToGetStepFromTransition(transitions.First(), out IStep nextStep))
            {
                Assert.Fail("First step is not the end of the chapter.");
            }

            Assert.Null(nextStep);
        }
    }
}
