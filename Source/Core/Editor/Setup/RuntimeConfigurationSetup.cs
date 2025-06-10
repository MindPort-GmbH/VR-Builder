// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Will setup a <see cref="RuntimeConfigurator"/> when none is existent in scene.
    /// </summary>
    internal class RuntimeConfigurationSetup : SceneSetup
    {
        public static readonly string ProcessConfigurationName = "PROCESS_CONFIGURATION";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                GameObject obj = new GameObject(ProcessConfigurationName);
                RuntimeConfigurator configurator = obj.AddComponent<RuntimeConfigurator>();
                configurator.SetRuntimeConfigurationName(configuration.RuntimeConfigurationName);
                SceneConfiguration sceneConfiguration = obj.AddComponent<SceneConfiguration>();
                sceneConfiguration.AddWhitelistAssemblies(configuration.AllowedExtensionAssemblies);
                sceneConfiguration.DefaultConfettiPrefab = configuration.DefaultConfettiPrefab;
                SetPrefabParent(obj, configuration.ParentObjectsHierarchy);
                Selection.activeObject = obj;
            }
        }
    }
}
