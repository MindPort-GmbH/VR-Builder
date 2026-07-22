// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Will setup a <see cref="RuntimeConfigurator"/> when none is existent in scene.
    /// </summary>
    internal class RuntimeConfigurationSetup : SceneSetup
    {
        private RuntimeConfigurator runtimeConfigurator;
        private ISceneService sceneService;
        private BaseModeHandler modeHandler;

        public static readonly string ProcessConfigurationName = "PROCESS_CONFIGURATION";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            if (ServiceRegistry.Has<RuntimeService>() == false)
            {
                var go = new GameObject(ProcessConfigurationName);

                runtimeConfigurator = go.AddComponent<RuntimeConfigurator>();
                ServiceRegistry.Get<RuntimeService>().Configurator = runtimeConfigurator;

                sceneService = go.AddComponent<SceneService>();
                sceneService.AddWhitelistAssemblies(configuration.AllowedExtensionAssemblies);
                sceneService.DefaultConfettiPrefab = configuration.DefaultConfettiPrefab;

                modeHandler = go.AddComponent<BaseModeHandler>();
                // modeHandler.AvailableModes =new List<IModeService>() { ActiveOrDefaultMode };
                ServiceRegistry.Get<IModeService>().ModeHandler = modeHandler;

                SetPrefabParent(go, configuration.ParentObjectsHierarchy);
                Selection.activeObject = go;
            }
        }
    }
}