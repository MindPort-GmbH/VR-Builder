// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using TinkerFlowDebug.addons.ProcessEngine.Source.Localization;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.StepLocking;
using VRBuilder.Core.User;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Orchestrates VRBuilder services by hooking up their locators and ensuring they are initialized with
    /// instances created from the configured settings. Each service manages its own configuration.
    /// </summary>
    /// <remarks>
    /// VRBuilder parts are modeled as services to make each part easier to configure.
    /// A service consists of a <b>Provider</b>, a <b>Configuration</b>, and a <b>Locator</b>,
    /// each split into an interface and an implementation.
    ///
    /// Implementations usually live directly in the Unity or Godot space, but sometimes they are general
    /// enough to reside in the engine-agnostic layer.
    ///
    /// <b>Provider</b><br/>
    /// The provider performs the service logic. For example: <see cref="IProcessRunner"/>
    /// runs the process and provides callbacks, while <see cref="IStepLockService"/> handles the locking
    /// and unlocking of steps.
    ///
    /// <b>Configuration</b><br/>
    /// The configuration provides whatever the provider needs. It also exposes <c>ServiceTypeName</c>,
    /// which is the full name (namespace + class name) of the <b>Provider</b>. This is important because
    /// the provider may live outside this engine-agnostic layer and we need a unique identifier. Since
    /// Unity and Godot each use the assembly-qualified name differently, we cannot use that here.
    /// Configurations follow the SettingsObject pattern of each engine
    /// and can have editor overrides that make the choice of <c>ServiceTypeName</c> easier.
    ///
    /// <b>Locator</b><br/>
    /// The locator is responsible for providing the service instance. <see cref="IVRBService"/>
    /// hooks up the locator for each service.
    /// </remarks>
    public static class VRBuilderServices
    {
        private static bool initialized;
        private static readonly Dictionary<Type, Type[]> implementationCache = new();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EnsureLoaded();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeLoad()
        {
            EnsureLoaded();
        }

        /// <summary>
        /// for each Service (Provider+Configuration+Locator) we need to ensure that it is loaded.
        /// </summary>
        public static void EnsureLoaded()
        {
            if (initialized) return;
            initialized = true;

            // Touch settings to ensure they exist (auto-creates if needed)
            TouchSettings();

            ProcessRunnerLocator.Current ??= CreateFromConfig<IProcessRunner>(
                ProcessRunnerSettings.Instance.ServiceTypeName,
                ProcessRunnerSettings.Instance);

            LanguageSettingsLocator.Current ??= CreateFromConfig<ILanguageService>(
                LanguageSettings.Instance.ServiceTypeName,
                LanguageSettings.Instance);

            // TODO: next RuntimeConfiguration could look like that
            // RuntimeConfiguratorLocator.CurrentConfig ??= CreateFromConfig<IRuntimeConfiguration>(
            //     RuntimeConfigurationSettings.Instance.RuntimeConfigurationName);

            StepLockLocator.Current ??= CreateFromConfig<IStepLockService>(
                StepLockSettings.Instance.ServiceTypeName,
                StepLockSettings.Instance);

            SceneObjectRegistryLocator.Current ??= CreateFromConfig<ISceneObjectRegistry>(
                SceneObjectRegistrySettings.Instance.ServiceTypeName,
                SceneObjectRegistrySettings.Instance);

            UserLocator.Current ??= CreateFromConfig<IUserService>(
                SceneObjectRegistrySettings.Instance.ServiceTypeName,
                SceneObjectRegistrySettings.Instance);
        }

        /// <summary>
        /// Touch all settings to ensure they exist. They all follow the Instance pattern so we can just call Instance and discard the value.
        /// </summary>
        private static void TouchSettings()
        {
            _ = ProcessRunnerSettings.Instance;
            // _ = RuntimeConfigurationSettings.Instance;
            _ = StepLockSettings.Instance;
            _ = SceneObjectRegistrySettings.Instance;
            _ = UserSettings.Instance;
        }

        /// <summary>
        /// Creates a Service by a specific pattern. It uses ReflectionUtils and also caches the Results, so restarting the Engine might help with Problems
        /// The pattern is:
        /// - look for concrete implementations of the interface.
        /// - check them agains the ServiceTypeName in the Configuration.
        /// - if found, create an instance of it.
        /// - set the Configuration based on the given Config
        /// - literally call Initialize() on the Service.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="config"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateFromConfig<T>(string typeName, object config = null) where T : class
        {
            if (!implementationCache.TryGetValue(typeof(T), out var implementations))
            {
                implementations = ReflectionUtils.GetConcreteImplementationsOf<T>().ToArray();
                implementationCache[typeof(T)] = implementations;
            }

            var match = implementations.FirstOrDefault(t => t.FullName == typeName);

            if (match == null)
            {
                Debug.LogError($"[VRBuilderServices] {typeof(T).Name} '{typeName}' not found. " +
                               $"Available: [{string.Join(", ", implementations.Select(t => t.FullName))}]. Falling back to first available.");
                match = implementations.FirstOrDefault();
            }

            if (match == null)
            {
                Debug.LogError($"[VRBuilderServices] No implementation of {typeof(T).Name} found.");
                return null;
            }

            if (Activator.CreateInstance(match) is IVRBService service && config != null)
            {
                service.SetConfiguration(config);
                service.Initialize();
                return (T)service;
            }

            Debug.LogError($"[VRBuilderServices] Failed to instantiate {match.Name}");
            return null;
        }
    }
}