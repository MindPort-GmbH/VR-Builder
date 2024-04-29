// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core.Configuration.Modes;
using NUnit.Framework;

namespace VRBuilder.Core.Tests.Utils
{
    public class ModeParameterTests
    {
        [Test]
        public void IsModifiedIsFlagged()
        {
            // Given a bool parameter
            ModeParameter<bool> parameter = new ModeParameter<bool>("p1", false);

            // When set to true
            parameter.Value = true;

            // Then the parameter is set to modified
            Assert.IsTrue(parameter.IsModified);
        }

        [Test]
        public void IsModifiedIsFalseOnReset()
        {
            // Given a bool parameter
            ModeParameter<bool> parameter = new ModeParameter<bool>("p1", false);
            parameter.Value = true;

            // When reset
            parameter.Reset();

            // Then is modified is false
            Assert.IsFalse(parameter.IsModified);
        }

        [Test]
        public void DefaultValueIsSet()
        {
            // Given a parameter with specific default value
            ModeParameter<int> parameter = new ModeParameter<int>("p1", 5);

            // Then the default value is set.
            Assert.AreEqual(5, parameter.Value);
        }

        [Test]
        public void DefaultValueIsSetAfterReset()
        {
            // Given a changed parameter with default value
            ModeParameter<int> parameter = new ModeParameter<int>("p1", 5);
            parameter.Value = 3;

            // When reset
            parameter.Reset();

            // Then it is the default value again.
            Assert.AreEqual(5, parameter.Value);
        }

        [Test]
        public void EventIsEmittedOnChange()
        {
            // Given a parameter with listener
            ModeParameter<int> parameter = new ModeParameter<int>("p1", 5);
            bool wasCalled = false;
            parameter.ParameterModified += (sender, args) => { wasCalled = true; };

            // When changed
            parameter.Value = 3;

            // Then the event was called
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void EventIsEmittedOnReset()
        {
            // Given a changed parameter with listener
            ModeParameter<int> parameter = new ModeParameter<int>("p1", 5);
            parameter.Value = 3;

            bool wasCalled = false;
            parameter.ParameterModified += (sender, args) => { wasCalled = true; };

            // When reset
            parameter.Reset();

            // Then event was called
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void EventIsNotEmittedWhenDefaultValueIsSetOnReset()
        {
            // Given a changed parameter with listener
            ModeParameter<int> parameter = new ModeParameter<int>("p1", 5);
            parameter.Value = 5;

            bool wasCalled = false;
            parameter.ParameterModified += (sender, args) => { wasCalled = true; };

            // When reset
            parameter.Reset();

            // Then event was called
            Assert.IsFalse(wasCalled);
        }
    }
}
