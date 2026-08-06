// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Input;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Unity.ProcessRunning;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Will setup a <see cref="RuntimeConfigurator"/> when none is existent in scene.
    /// </summary>
    internal class RuntimeConfigurationSetup : SceneSetup
    {
        private RuntimeConfigurator runtimeConfigurator;
        private DefaultProcessLoader processLoader;
        private SceneService sceneService;
        private BaseModeHandler modeHandler;
        private PlayerInput playerInput;

        public static readonly string ProcessConfigurationName = "PROCESS_CONFIGURATION";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            if (ServiceRegistry.Has<RuntimeService>())
            {
                var go = new GameObject(ProcessConfigurationName);

                runtimeConfigurator = go.AddComponent<RuntimeConfigurator>();
                runtimeConfigurator.RuntimeConfiguration = ScriptableObject.CreateInstance<RuntimeConfiguration>();
                ServiceRegistry.Get<RuntimeService>().Configurator = runtimeConfigurator;

                sceneService = go.AddComponent<SceneService>();
                sceneService.AddWhitelistAssemblies(configuration.AllowedExtensionAssemblies);
                sceneService.DefaultConfettiPrefab = configuration.DefaultConfettiPrefab;

                modeHandler = go.AddComponent<BaseModeHandler>();
                ServiceRegistry.Get<IModeService>().ModeHandler = modeHandler;

                playerInput = go.AddComponent<PlayerInput>();
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                playerInput.actions = (ServiceRegistry.Get<IInputController>() as InputController)?.CurrentInputActionAsset;

                SetPrefabParent(go, configuration.ParentObjectsHierarchy);
                Selection.activeObject = go;
            }
        }
    }
}
