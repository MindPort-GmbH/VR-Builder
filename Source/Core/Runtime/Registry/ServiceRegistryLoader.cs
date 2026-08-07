// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using Source.Core.Runtime.Configuration;
using Source.Core.Runtime.Localization;
using Source.TextToSpeech;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Input;
using VRBuilder.Core.IO;
using VRBuilder.Core.Localization;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.StepLocking;
using VRBuilder.Core.TextToSpeech;
using VRBuilder.Core.User;

namespace VRBuilder.Core.Runtime.Registry
{
    [CreateAssetMenu(fileName = "ServiceRegistry", menuName = "VR Builder/Service Registry", order = 1)]
    public class ServiceRegistryLoader : SettingsObject<ServiceRegistryLoader>
    {
        [Header("Services")]
        [SerializeField]
        [ServiceImplementation(typeof(IProcessRunner))]
        public string ProcessRunner = typeof(DefaultProcessRunner).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(ILanguageService))]
        public string LanguageService = typeof(LanguageService).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(IStepLockService))]
        public string StepLockService = typeof(DefaultStepLockHandling).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(ISceneObjectRegistry))]
        public string SceneObjectRegistry = typeof(SceneObjectRegistry).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(IUserService))]
        public string UserService = typeof(UserService).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(IModeService))]
        public string ModeService = typeof(ModeService).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(IInputController))]
        public string InputService = typeof(DefaultInputController).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(IPlatformFileSystem))]
        public string FileManager = typeof(FileManager).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(ITextToSpeechService))]
        public string TextToSpeechService = typeof(TextToSpeechService).AssemblyQualifiedName;

        [SerializeField]
        [ServiceImplementation(typeof(IRuntimeService))]
        public string RuntimeService = typeof(RuntimeService).AssemblyQualifiedName;

        [Header("Configurations")]
        [SerializeField]
        public ProcessRunnerSettings ProcessRunnerConfiguration;

        [SerializeField]
        public LanguageSettings LanguageConfiguration;

        [SerializeField]
        public StepLockSettings StepLockConfiguration;

        [SerializeField]
        public SceneObjectRegistrySettings SceneObjectRegistryConfiguration;

        [SerializeField]
        public ModeServiceSettings ModeServiceConfiguration;

        [SerializeField]
        public UserSettings UserConfiguration;

        [SerializeField]
        public InputSettings InputConfiguration;

        [SerializeField]
        public TextToSpeechProviderSettings TextToSpeechConfiguration;

        [SerializeField]
        public RuntimeServiceConfiguration RuntimeServiceConfiguration;

        private static bool initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeLoad()
        {
            Instance.Register();
        }

        public void Register()
        {
            if (initialized) return;

            ServiceRegistry.Register<IProcessRunner, IProcessRunnerConfiguration>(
                CreateService<IProcessRunner>(ProcessRunner),
                ProcessRunnerConfiguration ?? ProcessRunnerSettings.Instance);

            ServiceRegistry.Register<ILanguageService, ILanguageConfiguration>(
                CreateService<ILanguageService>(LanguageService),
                LanguageConfiguration ?? LanguageSettings.Instance);

            ServiceRegistry.Register<IModeService, IModeServiceConfiguration>(
                CreateService<IModeService>(ModeService),
                ModeServiceConfiguration ?? ModeServiceSettings.Instance);

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

            ServiceRegistry.Register(CreateService<IPlatformFileSystem>(FileManager));

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
