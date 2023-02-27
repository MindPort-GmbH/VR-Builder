// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Linq;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Unity;
using VRBuilder.Editor.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.Tests
{
    public class RuntimeConfigurationSetupTests
    {
        [Test]
        public void ConfigNotCreated()
        {
            // When the Runtime configuration setup is ran.
            new RuntimeConfigurationSetup().Setup();
            // Then there should be an GameObject with fitting name in scene.
            GameObject obj = GameObject.Find(RuntimeConfigurationSetup.ProcessConfigurationName);
            Assert.NotNull(obj);
        }

        [Test]
        public void IsConfigWithoutMissingScriptTest()
        {
            // When the Runtime configuration setup is ran.
            new RuntimeConfigurationSetup().Setup();
            // Then the found GameObject should not have missing scripts.
            GameObject obj = GameObject.Find(RuntimeConfigurationSetup.ProcessConfigurationName);
            Assert.NotNull(obj);
            Assert.False(SceneUtils.ContainsMissingScripts(obj));
        }
    }
}
