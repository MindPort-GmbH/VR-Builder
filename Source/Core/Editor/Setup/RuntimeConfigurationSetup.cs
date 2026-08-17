// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using Source.Core.Runtime.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Input;
using VRBuilder.Core.Localization;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Unity.ProcessRunning;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Will setup a <see cref="RuntimeHandler"/> when none is existent in scene.
    /// </summary>
    internal class RuntimeConfigurationSetup : SceneSetup
    {
        private RuntimeHandler runtimeHandler;
        private DefaultProcessHandler processHandler;
        private SceneService sceneService;
        private BaseModeHandler modeHandler;
        private LanguageHandler languageHandler;
        private PlayerInput playerInput;

        public static readonly string ProcessConfigurationName = "PROCESS_CONFIGURATION";

        /// <inheritdoc/>
        public override void Setup(ISceneSetupConfiguration configuration)
        {
            if (ServiceRegistry.Has<RuntimeService>())
            {
                var go = new GameObject(ProcessConfigurationName);

                processHandler = go.AddComponent<DefaultProcessHandler>();
                ServiceRegistry.Get<RuntimeService>().ProcessHandler = processHandler;

                runtimeHandler = go.AddComponent<RuntimeHandler>();
                runtimeHandler.RuntimeConfiguration = ScriptableObject.CreateInstance<RuntimeConfiguration>();
                ServiceRegistry.Get<RuntimeService>().Handler = runtimeHandler;

                sceneService = go.AddComponent<SceneService>();
                sceneService.AddWhitelistAssemblies(configuration.AllowedExtensionAssemblies);
                sceneService.DefaultConfettiPrefab = configuration.DefaultConfettiPrefab;

                modeHandler = new BaseModeHandler();
                ServiceRegistry.Get<IModeService>().ModeHandler = modeHandler;

                languageHandler = go.AddComponent<LanguageHandler>();
                ServiceRegistry.Get<ILanguageService>().LanguageHandler = languageHandler;

                playerInput = go.AddComponent<PlayerInput>();
                playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
                playerInput.actions = (ServiceRegistry.Get<IInputController>() as InputController)?.CurrentInputActionAsset;

                SetPrefabParent(go, configuration.ParentObjectsHierarchy);
                Selection.activeObject = go;
            }
        }
    }
}
