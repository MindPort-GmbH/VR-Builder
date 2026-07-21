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
using VRBuilder.Core.StepLocking;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Configuration
{
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

        public static void EnsureLoaded()
        {
            if (initialized) return;
            initialized = true;

            // Touch settings to ensure they exist (auto-creates if needed)
            TouchSettings();

            ProcessRunnerLocator.Current ??= CreateFromConfig<IProcessRunner>(
                ProcessRunnerSettings.Instance.RunnerName,
                ProcessRunnerSettings.Instance);

            LanguageSettingsLocator.Current ??= CreateFromConfig<ILanguageService>(
                LanguageSettings.Instance.ServiceTypeName,
                LanguageSettings.Instance);

            // TODO: next RuntimeConfiguration could look like that
            // RuntimeConfiguratorLocator.CurrentConfig ??= CreateFromConfig<IRuntimeConfiguration>(
            //     RuntimeConfigurationSettings.Instance.RuntimeConfigurationName);

            StepLockLocator.Current ??= CreateFromConfig<IStepLockService>(
                StepLockSettings.Instance.StepLockHandlingTypeName,
                StepLockSettings.Instance);
        }

        private static void TouchSettings()
        {
            _ = ProcessRunnerSettings.Instance;
            // _ = RuntimeConfigurationSettings.Instance;
            _ = StepLockSettings.Instance;
        }

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
