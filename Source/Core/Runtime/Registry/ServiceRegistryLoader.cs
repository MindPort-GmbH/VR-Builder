// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Source.Core.Runtime.Localization;
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
using VRBuilder.Core.User;
using VRBuilder.Core.Utils;
using VRBuilder.Unity.ProcessRunning;
using ModeService = VRBuilder.Core.Configuration.Modes.ModeService;

namespace VRBuilder.Core.Runtime.Registry
{
    [CreateAssetMenu(fileName = "ServiceRegistry", menuName = "VR Builder/Service Registry", order = 1)]
    public class ServiceRegistryLoader : SettingsObject<ServiceRegistryLoader>
    {
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
        [ServiceImplementation(typeof(IRuntimeService))]
        public string RuntimeService = typeof(RuntimeService).FullName;

        private static bool initialized;
        private static readonly Dictionary<Type, Type[]> implementationCache = new();

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

            ServiceRegistry.Register(CreateService<IProcessRunner>(ProcessRunner));
            ServiceRegistry.Register(CreateService<ILanguageService>(LanguageService));
            ServiceRegistry.Register(CreateService<IModeService>(ModeService));
            ServiceRegistry.Register(CreateService<IStepLockService>(StepLockService));
            ServiceRegistry.Register(CreateService<ISceneObjectRegistry>(SceneObjectRegistry));
            ServiceRegistry.Register(CreateService<IUserService>(UserService));
            ServiceRegistry.Register(CreateService<IInputController>(InputService));
            ServiceRegistry.Register(CreateService<IPlatformFileSystem>(FileManager));

            initialized = true;
        }

        private T CreateService<T>(string typeName) where T : class
        {
            if (!implementationCache.TryGetValue(typeof(T), out var implementations))
            {
                implementations = ReflectionUtils.GetConcreteImplementationsOf<T>().ToArray();
                implementationCache[typeof(T)] = implementations;
            }

            var match = implementations.FirstOrDefault(t => t.FullName == typeName);

            if (match == null)
            {
                Debug.LogError($"[{nameof(ServiceRegistryLoader)}] {typeof(T).Name} '{typeName}' not found. " +
                               $"Available: [{string.Join(", ", implementations.Select(t => t.FullName))}]. Falling back to first available.");
                match = implementations.FirstOrDefault();
            }

            if (match != null)
            {
                var service = Activator.CreateInstance(match) as T;
                TryInjectConfiguration(service);
                return service;
            }

            return null;
        }

        private static void TryInjectConfiguration<T>(T service) where T : class
        {
            var serviceType = service.GetType();

            var serviceInterface = serviceType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IService<>));

            if (serviceInterface == null) return;

            var configType = serviceInterface.GetGenericArguments()[0];

            var settingsType = ReflectionUtils.GetAllTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract
                                               && configType.IsAssignableFrom(t)
                                               && IsSettingsObject(t));

            var configInstance = settingsType
                ?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);

            if (configInstance == null) return;

            var setConfig = serviceType.GetMethod("SetConfiguration",
                BindingFlags.Public | BindingFlags.Instance, null,
                new[] { configType }, null);

            setConfig?.Invoke(service, new[] { configInstance });
        }

        private static bool IsSettingsObject(Type type)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.IsGenericType
                    && current.GetGenericTypeDefinition() == typeof(SettingsObject<>))
                    return true;
                current = current.BaseType;
            }

            return false;
        }
    }
}
