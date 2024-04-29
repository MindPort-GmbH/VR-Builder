// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using NUnit.Framework;
using UnityEngine;
using VRBuilder.Editor.Setup;
using VRBuilder.Unity;

namespace VRBuilder.Editor.Tests
{
    public class RuntimeConfigurationSetupTests
    {
        [Test]
        public void ConfigNotCreated()
        {
            // When the Runtime configuration setup is ran.
            new RuntimeConfigurationSetup().Setup(new DefaultSceneSetupConfiguration());
            // Then there should be an GameObject with fitting name in scene.
            GameObject obj = GameObject.Find(RuntimeConfigurationSetup.ProcessConfigurationName);
            Assert.NotNull(obj);
        }

        [Test]
        public void IsConfigWithoutMissingScriptTest()
        {
            // When the Runtime configuration setup is ran.
            new RuntimeConfigurationSetup().Setup(new DefaultSceneSetupConfiguration());
            // Then the found GameObject should not have missing scripts.
            GameObject obj = GameObject.Find(RuntimeConfigurationSetup.ProcessConfigurationName);
            Assert.NotNull(obj);
            Assert.False(SceneUtils.ContainsMissingScripts(obj));
        }
    }
}
