// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.Collections.Generic;
using VRBuilder.Core;
using VRBuilder.Editor.UI.Windows;
using NUnit.Framework;

namespace VRBuilder.Editor.Tests.ProcessWindowTests
{
    internal class CopyOneStepWithContextMenuTest : BaseProcessWindowTest
    {
        public override string WhenDescription => "Add one step to the workflow. Copy it with RMB -> Copy Step, RMB -> Paste step.";

        public override string ThenDescription => "Then there are two identical steps.";

        protected override void Then(ProcessWindow window)
        {
            IList<IStep> steps = window.GetProcess().Data.FirstChapter.Data.Steps;
            Assert.AreEqual(2, steps.Count);
            Assert.AreEqual("Copy of " + steps[0].Data.Name, steps[1].Data.Name);
            Assert.True(steps[0].Data.Behaviors.Data.Behaviors.Count == 0);
            Assert.True(steps[1].Data.Behaviors.Data.Behaviors.Count == 0);
            Assert.True(steps[0].Data.Transitions.Data.Transitions.Count == 1);
            Assert.True(steps[1].Data.Transitions.Data.Transitions.Count == 1);
            Assert.Null(steps[0].Data.Transitions.Data.Transitions[0].Data.TargetStep);
            Assert.Null(steps[1].Data.Transitions.Data.Transitions[0].Data.TargetStep);
        }
    }
}