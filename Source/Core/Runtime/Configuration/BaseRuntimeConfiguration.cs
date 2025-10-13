// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.IO;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Serialization;
using VRBuilder.Core.Utils;
using VRBuilder.UI.Console;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Base class for your runtime process configuration. Extend it to create your own.
    /// </summary>
#pragma warning disable 0618
    public abstract class BaseRuntimeConfiguration
    {
#pragma warning restore 0618
        /// <summary>
        /// Name of the manifest file that could be used to save process asset information.
        /// </summary>
        public static string ManifestFileName => "ProcessManifest";

        private ISceneObjectRegistry sceneObjectRegistry = new GuidBasedSceneObjectRegistry();
        private ISceneConfiguration sceneConfiguration;

        /// <inheritdoc />
        public virtual ISceneObjectRegistry SceneObjectRegistry
        {
            get
            {
                if (sceneObjectRegistry == null)
                {
                    sceneObjectRegistry = new GuidBasedSceneObjectRegistry();
                }

                return sceneObjectRegistry;
            }
        }

        /// <inheritdoc />
        public IProcessSerializer Serializer { get; set; } = new NewtonsoftJsonProcessSerializerV4();

        /// <summary>
        /// Default input action asset which is used when no customization of key bindings are done.
        /// Should be stored inside the VR Builder package.
        /// </summary>
        public virtual string DefaultInputActionAssetPath { get; } = "KeyBindings/BuilderDefaultKeyBindings";

        /// <summary>
        /// Custom InputActionAsset path which is used when key bindings are modified.
        /// Should be stored in project path.
        /// </summary>
        public virtual string CustomInputActionAssetPath { get; } = "KeyBindings/BuilderCustomKeyBindings";

#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE
        private UnityEngine.InputSystem.InputActionAsset inputActionAsset;

        /// <summary>
        /// Current active InputActionAsset.
        /// </summary>
        public virtual UnityEngine.InputSystem.InputActionAsset CurrentInputActionAsset
        {
            get
            {
                if (inputActionAsset == null)
                {
                    inputActionAsset = Resources.Load<UnityEngine.InputSystem.InputActionAsset>(CustomInputActionAssetPath);
                    if (inputActionAsset == null)
                    {
                        inputActionAsset = Resources.Load<UnityEngine.InputSystem.InputActionAsset>(DefaultInputActionAssetPath);
                    }
                }

                return inputActionAsset;
            }

            set => inputActionAsset = value;
        }
#endif

        /// <inheritdoc />
        public IModeHandler Modes { get; protected set; }

        /// <inheritdoc />
        [Obsolete("Use User property instead.")]
        public abstract UserSceneObject LocalUser { get; }

        /// <summary>
        /// Returns transform positions of the local user's head, hands and root.
        /// </summary>
        public abstract IXRRigTransform User { get; }

        /// <inheritdoc />
        public abstract AudioSource InstructionPlayer { get; }

        /// <summary>
        /// Determines the property locking strategy used for this runtime configuration.
        /// </summary>
        public StepLockHandlingStrategy StepLockHandling { get; set; }

        /// <summary>
        /// Returns transform data for all user rigs in the scene.
        /// </summary>
        public abstract IEnumerable<IXRRigTransform> UserTransforms { get; }

        /// <inheritdoc />
        public abstract IProcessAudioPlayer ProcessAudioPlayer { get; }

        /// <inheritdoc />
        public abstract ISceneObjectManager SceneObjectManager { get; }

        public virtual string WorldConsolePrefab => "Prefabs/DefaultWorldConsole";

        protected ILogConsole logConsole;

        public ILogConsole WorldConsole
        {
            get
            {
                if (logConsole == null)
                {
                    GameObject logConsoleObj = GameObject.Instantiate(Resources.Load<GameObject>(WorldConsolePrefab));
                    logConsole = logConsoleObj.GetComponent<ILogConsole>();
                }

                if (logConsole == null)
                {
                    throw new NullReferenceException("Failed to load world console prefab.");
                }

                return logConsole;
            }
        }

        /// <inheritdoc />
        public virtual ISceneConfiguration SceneConfiguration
        {
            get
            {
                if (sceneConfiguration == null)
                {
                    ISceneConfiguration configuration = RuntimeConfigurator.Instance.gameObject.GetComponent<ISceneConfiguration>();

                    if (configuration == null)
                    {
                        configuration = RuntimeConfigurator.Instance.gameObject.AddComponent<SceneConfiguration>();
                    }

                    sceneConfiguration = configuration;
                }

                return sceneConfiguration;
            }
        }

        protected BaseRuntimeConfiguration() : this(new DefaultStepLockHandling())
        {
        }

        protected BaseRuntimeConfiguration(StepLockHandlingStrategy lockHandling)
        {
            StepLockHandling = lockHandling;
        }

        /// <inheritdoc />
        public virtual async Task<IProcess> LoadProcess(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("Given path is null or empty!");
                }

                int index = path.LastIndexOf("/");
                string processFolder = path.Substring(0, index);
                string processName = GetProcessNameFromPath(path);
                string manifestPath = $"{processFolder}/{ManifestFileName}.{Serializer.FileFormat}";

                IProcessAssetManifest manifest;

#if !UNITY_EDITOR && UNITY_WEBGL
                manifest = FetchManifestWebGL(processName, manifestPath);
#else
                manifest = await FetchManifest(processName, manifestPath);
#endif
                IProcessAssetStrategy assetStrategy = ReflectionUtils.CreateInstanceOfType(ReflectionUtils.GetConcreteImplementationsOf<IProcessAssetStrategy>().FirstOrDefault(type => type.FullName == manifest.AssetStrategyTypeName)) as IProcessAssetStrategy;

                string processAssetPath = $"{processFolder}/{manifest.ProcessFileName}.{Serializer.FileFormat}";
                byte[] processData = await FileManager.Read(processAssetPath);
                List<byte[]> additionalData = await GetAdditionalProcessData(processFolder, manifest);

                return assetStrategy.GetProcessFromSerializedData(processData, additionalData, Serializer);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Error when loading process. {exception.GetType().Name}, {exception.Message}\n{exception.StackTrace}", RuntimeConfigurator.Instance.gameObject);
            }

            return null;
        }

        private async Task<List<byte[]>> GetAdditionalProcessData(string processFolder, IProcessAssetManifest manifest)
        {
            List<byte[]> additionalData = new List<byte[]>();
            foreach (string fileName in manifest.AdditionalFileNames)
            {
                string filePath = $"{processFolder}/{fileName}.{Serializer.FileFormat}";

                if (await FileManager.Exists(filePath))
                {
                    additionalData.Add(await FileManager.Read(filePath));
                }
                else
                {
                    Debug.Log($"Error loading process. File not found: {filePath}");
                }
            }

            return additionalData;
        }

        private async Task<IProcessAssetManifest> FetchManifest(string processName, string manifestPath)
        {
            IProcessAssetManifest manifest;

            if (await FileManager.Exists(manifestPath))
            {
                byte[] manifestData = await FileManager.Read(manifestPath);
                manifest = Serializer.ManifestFromByteArray(manifestData);
            }
            else
            {
                manifest = new ProcessAssetManifest()
                {
                    AssetStrategyTypeName = typeof(SingleFileProcessAssetStrategy).FullName,
                    ProcessFileName = processName,
                    AdditionalFileNames = Array.Empty<string>(),
                };
            }

            return manifest;
        }

        private IProcessAssetManifest FetchManifestWebGL(string processName, string manifestPath)
        {
            IProcessAssetManifest manifest = new ProcessAssetManifest()
            {
                AssetStrategyTypeName = typeof(SingleFileProcessAssetStrategy).FullName,
                ProcessFileName = processName,
                AdditionalFileNames = Array.Empty<string>(),
            };
            return manifest;
        }

        private static string GetProcessNameFromPath(string path)
        {
            int slashIndex = path.LastIndexOf('/');
            string fileName = path.Substring(slashIndex + 1);
            int pointIndex = fileName.LastIndexOf('.');
            fileName = fileName.Substring(0, pointIndex);

            return fileName;
        }
    }
}
