// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Source.Core.Runtime.Configuration;
using Source.Core.Runtime.Configuration;
using Source.Core.Runtime.Localization;
using Source.TextToSpeech;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Input;
using VRBuilder.Core.IO;
using VRBuilder.Core.Localization;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.Registry;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.StepLocking;
using VRBuilder.Core.TextToSpeech;
using VRBuilder.Core.User;
using VRBuilder.Unity.ProcessRunning;
using ModeService = VRBuilder.Core.Configuration.Modes.ModeService;

namespace VRBuilder.Core.Runtime.Registry
{
    [CreateAssetMenu(fileName = "ServiceRegistry", menuName = "VR Builder/Service Registry", order = 1)]
    public class ServiceRegistryLoader : SettingsObject<ServiceRegistryLoader>
    {
        [Header("Services")]
        [SerializeField]
        [ServiceImplementation(typeof(IProcessRunner))]
        public string ProcessRunner = typeof(DefaultProcessRunner).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(ILanguageService))]
        public string LanguageService = typeof(LanguageService).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(IStepLockService))]
        public string StepLockService = typeof(DefaultStepLockHandling).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(ISceneObjectRegistry))]
        public string SceneObjectRegistry = typeof(SceneObjectRegistry).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(IUserService))]
        public string UserService = typeof(UserService).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(IModeService))]
        public string ModeService = typeof(ModeService).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(IInputController))]
        public string InputService = typeof(DefaultInputController).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(IPlatformFileSystem))]
        public string FileManager = typeof(FileManager).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(ITextToSpeechService))]
        public string TextToSpeechService = typeof(TextToSpeechService).FullName;

        [SerializeField]
        [ServiceImplementation(typeof(IRuntimeService))]
        public string RuntimeService = typeof(RuntimeService).FullName;

        [Header("Configurations")]
        [SerializeField]
        public ProcessRunnerSettings ProcessRunnerConfiguration;

        [SerializeField]
        public LanguageSettings LanguageConfiguration;

        [SerializeField]
        public ModeSettings ModeConfiguration;

        [SerializeField]
        public StepLockSettings StepLockConfiguration;

        [SerializeField]
        public SceneObjectRegistrySettings SceneObjectRegistryConfiguration;

        [SerializeField]
        public UserSettings UserConfiguration;

        [SerializeField]
        public InputSettings InputConfiguration;

        [SerializeField]
        public TextToSpeechProviderSettings TextToSpeechConfiguration;

        [SerializeField]
        public RuntimeServiceConfiguration RuntimeServiceConfiguration;

        private static bool initialized;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            Instance.Register();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeLoad()
        {
            Instance.Register();
        }

        private void Register()
        {
            if (initialized) return;

            ServiceRegistry.Register<IProcessRunner, IProcessRunnerConfiguration>(
                CreateService<IProcessRunner>(ProcessRunner),
                ProcessRunnerConfiguration ?? ProcessRunnerSettings.Instance);

            ServiceRegistry.Register<ILanguageService, ILanguageConfiguration>(
                CreateService<ILanguageService>(LanguageService),
                LanguageConfiguration ?? LanguageSettings.Instance);

            ServiceRegistry.Register<IModeService>(CreateService<IModeService>(ModeService));

            ServiceRegistry.Register<IStepLockService, IStepLockConfiguration>(
                CreateService<IStepLockService>(StepLockService),
                StepLockConfiguration ?? StepLockSettings.Instance);

            ServiceRegistry.Register<ISceneObjectRegistry, ISceneObjectRegistryConfiguration>(
                CreateService<ISceneObjectRegistry>(SceneObjectRegistry),
                SceneObjectRegistryConfiguration ?? SceneObjectRegistrySettings.Instance);

            ServiceRegistry.Register<IUserService, IUserConfiguration>(
                CreateService<IUserService>(UserService),
                UserConfiguration ?? UserSettings.Instance);

            ServiceRegistry.Register<IInputController, IInputConfiguration>(
                CreateService<IInputController>(InputService),
                InputConfiguration ?? InputSettings.Instance);

            ServiceRegistry.Register<IPlatformFileSystem>(CreateService<IPlatformFileSystem>(FileManager));

            ServiceRegistry.Register<ITextToSpeechService, ITextToSpeechConfiguration>(
                CreateService<ITextToSpeechService>(TextToSpeechService),
                TextToSpeechConfiguration ?? TextToSpeechProviderSettings.Instance);

            ServiceRegistry.Register<IRuntimeService, IRuntimeServiceConfiguration>(
                CreateService<IRuntimeService>(RuntimeService),
                RuntimeServiceConfiguration ?? RuntimeServiceConfiguration.Instance);
            initialized = true;
        }

        private static T CreateService<T>(string typeName) where T : class
        {
            var resolvedType = Type.GetType(typeName);
            if (resolvedType == null)
            {
                Debug.LogError($"[{nameof(ServiceRegistryLoader)}] {typeof(T).Name} type '{typeName}' could not be resolved.");
                return null;
            }

            return Activator.CreateInstance(resolvedType) as T;
        }
    }
}