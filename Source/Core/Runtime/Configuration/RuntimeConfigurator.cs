// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Utils;
using VRBuilder.Core.SceneObjects;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Configurator to set the process runtime configuration which is used by a process during its execution.
    /// There has to be one and only one process runtime configurator game object per scene.
    /// </summary>
    public sealed class RuntimeConfigurator : MonoBehaviour
    {
        /// <summary>
        /// The event that fires when a process mode or runtime configuration changes.
        /// </summary>
        public static event EventHandler<ModeChangedEventArgs> ModeChanged;

        /// <summary>
        /// The event that fires when a process runtime configuration changes.
        /// </summary>
        public static event EventHandler<EventArgs> RuntimeConfigurationChanged;

        /// <summary>
        /// Fully qualified name of the runtime configuration used.
        /// </summary>
        /// <remarks>
        /// This field is filled by <see cref="RuntimeConfiguratorEditor"/>
        /// </remarks>
        [SerializeField]
        private string runtimeConfigurationName = typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName;

        /// <summary>
        /// Process name which is selected.
        /// </summary>
        /// <remarks>
        /// This field is filled by <see cref="RuntimeConfiguratorEditor"/>
        /// </remarks>
        [SerializeField]
        private string selectedProcessStreamingAssetsPath = "";

        /// <summary>
        /// String localization table used by the current process.
        /// </summary>
        [SerializeField]
        private string processStringLocalizationTable = "";

        private BaseRuntimeConfiguration runtimeConfiguration;

        private static RuntimeConfigurator instance;
        private static RuntimeConfigurator[] instances;

#if UNITY_EDITOR
        /// <summary>
        /// Flag to track if there are any dirty scene objects that need registry refresh.
        /// </summary>
        /// <remarks>
        /// Tracks state swishes of prefabs between edit mode and the main scene.
        /// </remarks>
        private static bool hasDirtySceneObjects = false;

        /// <summary>
        /// Marks a scene object's prefab as dirty, indicating that the registry needs refreshing.
        /// </summary>
        /// <param name="sceneObject">The scene object to mark as dirty (currently not used).</param>
        /// <remarks>
        /// This currently triggers a full refresh of the SceneObjectRegistry
        /// </remarks>
        public static void MarkSceneObjectDirty(ProcessSceneObject sceneObject)
        {
            //TODO Keep track of all changed prefabs in a separate class ot the SceneObjectRegistry and only update those
            hasDirtySceneObjects = true;
        }
#endif

        /// <summary>
        /// Looks up all the RuntimeConfigurator game objects in the scene.
        /// </summary>
        /// <returns>An array of RuntimeConfigurator instances found in the scene.</returns>
        private static RuntimeConfigurator[] LookUpRuntimeConfiguratorGameObjects()
        {
            RuntimeConfigurator[] instances = FindObjectsByType<RuntimeConfigurator>(FindObjectsSortMode.None);

            return instances;
        }

        /// <summary>
        /// Gets the RuntimeConfigurator instance.
        /// </summary>
        /// <returns>The active RuntimeConfigurator instance.</returns>
        /// <remarks>
        /// If there are multiple instances of the RuntimeConfigurator in the scene, the first one found will be used and
        /// a warning will be logged. 
        /// </remarks>
        private static RuntimeConfigurator GetRuntimeConfigurator()
        {
            RuntimeConfigurator[] instances = LookUpRuntimeConfiguratorGameObjects();

            if (instances.Length == 0)
            {
                return null;
            }
            if (instances.Length > 1)
            {
                string errorLog = $"More than one process runtime configurator found in all open scenes. The active process will be {instances[0].GetSelectedProcess()} from Scene {instances[0].gameObject.scene.name}. Ignoring following processes: ";
                for (int i = 1; i < instances.Length; i++)
                {
                    errorLog += $"{instances[i].GetSelectedProcess()} from Scene {instances[i].gameObject.scene.name}";
                    if (i < instances.Length - 1)
                    {
                        errorLog += ", ";
                    }
                }
                Debug.LogWarning(errorLog);
            }

            return instances[0];
        }

        /// <summary>
        /// Checks if a process runtime configurator exists in the scene.
        /// </summary>
        /// <returns><c>true</c> if an instance of the runtime configurator exists; otherwise, <c>false</c>.</returns>
        public static bool Exists
        {
            get
            {
                return IsExisting();
            }
        }

        /// <summary>
        /// Checks if an instance of the runtime configurator exists.
        /// If <see cref="instance"/> is not set it tries to set it by calling <see cref="LookUpRuntimeConfiguratorGameObject"/>.
        /// </summary>
        /// <param name="forceNewLookup">If set to <c>true</c>, forces a new lookup for the instance.</param>
        /// <returns><c>true</c> if an instance of the runtime configurator exists; otherwise, <c>false</c>.</returns>
        public static bool IsExisting(bool forceNewLookup = false)
        {
            if (instance == null || instance.Equals(null) || forceNewLookup)
            {
                instance = GetRuntimeConfigurator();
            }

            return instance != null && instance.Equals(null) == false;
        }

        /// <summary>
        /// Shortcut to get the <see cref="IRuntimeConfiguration"/> of the instance.
        /// </summary>
        public static BaseRuntimeConfiguration Configuration
        {
            get
            {
                if (Instance?.runtimeConfiguration != null)
                {
                    return Instance.runtimeConfiguration;
                }

                Type type = ReflectionUtils.GetTypeFromAssemblyQualifiedName(Instance?.runtimeConfigurationName);

                if (type == null)
                {
                    Debug.LogError($"IRuntimeConfiguration type '{Instance?.runtimeConfigurationName}' cannot be found. Using '{typeof(DefaultRuntimeConfiguration).AssemblyQualifiedName}' instead.");
                    type = typeof(DefaultRuntimeConfiguration);
                }

                Configuration = (BaseRuntimeConfiguration)ReflectionUtils.CreateInstanceOfType(type);

                return Instance?.runtimeConfiguration;
            }
            set
            {
                if (value == null)
                {
                    Debug.LogError("Process runtime configuration cannot be null.");
                    return;
                }

                if (Instance?.runtimeConfiguration == value)
                {
                    return;
                }

                if (Instance?.runtimeConfiguration != null)
                {
                    Instance.runtimeConfiguration.Modes.ModeChanged -= RuntimeConfigurationModeChanged;
                }

                value.Modes.ModeChanged += RuntimeConfigurationModeChanged;

                if (Instance)
                {
                    Instance.runtimeConfigurationName = value.GetType().AssemblyQualifiedName;
                    Instance.runtimeConfiguration = value;

                    EmitRuntimeConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Gets the current instance of the RuntimeConfigurator.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown if there is no RuntimeConfigurator added to the scene.</exception>
        public static RuntimeConfigurator Instance
        {
            get
            {
                if (Exists == false)
                {
                    throw new NullReferenceException("Process runtime configurator is not set in the scene. Create an empty game object with the 'RuntimeConfigurator' script attached to it.");
                }
                return instance;
            }
        }

        /// <summary>
        /// Returns the assembly qualified name of the runtime configuration.
        /// </summary>
        /// <returns>The assembly qualified name of the runtime configuration.</returns>
        public string GetRuntimeConfigurationName()
        {
            return runtimeConfigurationName;
        }

        /// <summary>
        /// Sets the runtime configuration name, expects an assembly qualified name.
        /// </summary>
        /// <param name="configurationName">The assembly qualified name of the runtime configuration.</param>
        public void SetRuntimeConfigurationName(string configurationName)
        {
            runtimeConfigurationName = configurationName;
        }

        /// <summary>
        /// Returns the path to the selected process.
        /// </summary>
        /// <returns>The path to the selected process.</returns>
        public string GetSelectedProcess()
        {
            return selectedProcessStreamingAssetsPath;
        }

        /// <summary>
        /// Sets the path to the selected process.
        /// </summary>
        /// <param name="path">The path to the selected process.</param>
        public void SetSelectedProcess(string path)
        {
            selectedProcessStreamingAssetsPath = path;
        }

        /// <summary>
        /// Returns the string localization table for the selected process.
        /// </summary>
        /// <returns>The string localization table for the selected process.</returns>
        public string GetProcessStringLocalizationTable()
        {
            return processStringLocalizationTable;
        }

        private void Awake()
        {
            Configuration.SceneObjectRegistry.RegisterAll();
            RuntimeConfigurationChanged += HandleRuntimeConfigurationChanged;
        }

        private void OnDestroy()
        {
            ModeChanged = null;
            RuntimeConfigurationChanged = null;
        }

        private void Update()
        {
#if UNITY_EDITOR
            RefreshRegistryIfNeeded();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Refreshes the scene object registry if there are dirty scene objects that need to be updated.
        /// </summary>
        private void RefreshRegistryIfNeeded()
        {
            // TODO Potentially move this into SceneObjectRegistry. Including the hasDirtySceneObjects flag and MarkSceneObjectDirty(object sceneObject).
            if (Exists && Configuration?.SceneObjectRegistry != null)
            {
                bool isMainStage = StageUtility.GetCurrentStageHandle() == StageUtility.GetMainStageHandle();

                if (isMainStage && hasDirtySceneObjects)
                {
                    Configuration.SceneObjectRegistry.Refresh();
                    hasDirtySceneObjects = false;
                }
            }
        }
#endif

        private static void EmitModeChanged()
        {
            ModeChanged?.Invoke(Instance, new ModeChangedEventArgs(Instance.runtimeConfiguration.Modes.CurrentMode));
        }

        private static void EmitRuntimeConfigurationChanged()
        {
            RuntimeConfigurationChanged?.Invoke(Instance, EventArgs.Empty);
        }

        private void HandleRuntimeConfigurationChanged(object sender, EventArgs args)
        {
            EmitModeChanged();
        }

        private static void RuntimeConfigurationModeChanged(object sender, ModeChangedEventArgs modeChangedEventArgs)
        {
            EmitModeChanged();
        }
    }
}
