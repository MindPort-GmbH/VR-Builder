// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using VRBuilder.Editor.UI.Windows;
using NUnit.Framework;

namespace VRBuilder.Core.Tests.Editor.StepWindowTests
{
    internal class RenameTest : BaseStepWindowTest
    {
        public override string WhenDescription => "Rename step to \"New Step!\" (without quotes).";
        public override string ThenDescription => "The step's name is \"New Step!\".";
        protected override void Then(StepWindow window)
        {
            IStep step = window.GetStep();
            Assert.AreEqual("New Step!", step.Data.Name);
        }
    }
}