// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRBuilder.Core.IO;
using VRBuilder.Core.ProcessRunning;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Serialization;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Utils.Logging;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Default runtime service to get and load processes over the <see cref="RuntimeHandler"/> to the scene.
    /// </summary>
    public class RuntimeService : IRuntimeService
    {
        private Action<string?> selectedProcessChanged;

        private IRuntimeServiceConfiguration configuration;
        private IRuntimeHandler? runtimeHandler;
        private IConfigurableProcessHandler? processHandler;
        private string selectedProcessStreamingAssetsPath;

        public event Action<string?> SelectedProcessChanged
        {
            add => selectedProcessChanged += value;
            remove => selectedProcessChanged -= value;
        }

        /// <inheritdoc />
        public ILifeCycleLoggingConfiguration LifeCycleLogging => LifeCycleLoggingConfig.Instance;

        /// <inheritdoc />
        public IRuntimeHandler Handler
        {
            get => runtimeHandler;
            set
            {
                string previousSelection = SelectedProcess;

                if (runtimeHandler is RuntimeHandler previousConfigurator)
                {
                    previousConfigurator.SelectedProcessChanged -= OnSelectedProcessChanged;
                }

                runtimeHandler = value;

                if (runtimeHandler is RuntimeHandler currentConfigurator)
                {
                    currentConfigurator.SelectedProcessChanged += OnSelectedProcessChanged;
                }

                string currentSelection = runtimeHandler?.RuntimeConfiguration?.SelectedProcess ?? string.Empty;
                if (string.Equals(previousSelection, currentSelection, StringComparison.Ordinal) == false)
                {
                    selectedProcessChanged?.Invoke(currentSelection);
                }
            }
        }

        /// <inheritdoc />
        public string SelectedProcess
        {
            get => runtimeHandler?.SelectedProcess;
            set
            {
                if (runtimeHandler == null || (runtimeHandler.SelectedProcess ?? string.Empty) == (value ?? string.Empty))
                {
                    ForwardingLogger.LogError("The process handler is null or selected process is null or empty.");
					return;
                }

				runtimeHandler.SelectedProcess = value;
                selectedProcessChanged?.Invoke(value);
            }
        }

        /// <inheritdoc />
        public IConfigurableProcessHandler ProcessHandler
        {
            get => processHandler;
            set => processHandler = value;
        }

        /// <inheritdoc />
        public string SelectedProcessStreamingAssetsPath
        {
            get => configuration.SelectedProcessStreamingAssetsPath;
            set => configuration.SelectedProcessStreamingAssetsPath = value;
        }

        /// <inheritdoc />
        public IProcessSerializer Serializer { get; set; } = new NewtonsoftJsonProcessSerializerV4();

        /// <inheritdoc />
        public string ManifestFileName
        {
            get => runtimeHandler?.RuntimeConfiguration?.ManifestFileName ?? "ProcessManifest";
            set
            {
                if (runtimeHandler != null)
                {
                    runtimeHandler.RuntimeConfiguration.ManifestFileName = value;
                }
            }
        }

        /// <inheritdoc />
        public void SetConfiguration(IRuntimeServiceConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc />
        public async Task<IProcess> LoadProcess(string path = "")
        {
            if(string.IsNullOrEmpty(path))
            {
                path = SelectedProcess;
            }

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("Given path is null or empty!");
                }

                int index = path.LastIndexOf("/", StringComparison.Ordinal);
                string processFolder = path.Substring(0, index);
                string processName = GetProcessNameFromPath(path);
                string manifestPath = $"{processFolder}/{ManifestFileName}.{Serializer.FileFormat}";

                var manifest = await ServiceRegistry.Get<IPlatformFileSystem>().FetchManifest(processName, manifestPath, Serializer);

                IProcessAssetStrategy assetStrategy = ReflectionUtils.CreateInstanceOfType(ReflectionUtils.GetConcreteImplementationsOf<IProcessAssetStrategy>().FirstOrDefault(type => type.FullName == manifest.AssetStrategyTypeName)) as IProcessAssetStrategy;

                string processAssetPath = $"{processFolder}/{manifest.ProcessFileName}.{Serializer.FileFormat}";
                byte[] processData = await ServiceRegistry.Get<IPlatformFileSystem>().Read(processAssetPath);
                List<byte[]> additionalData = await GetAdditionalProcessData(processFolder, manifest);

                return assetStrategy.GetProcessFromSerializedData(processData, additionalData, Serializer);
            }
            catch (Exception exception)
            {
                ForwardingLogger.LogError($"Error when loading process. {exception.GetType().Name}, {exception.Message}\n{exception.StackTrace},{ServiceRegistry.Get<RuntimeService>()}");
            }

            return null;
        }

        /// <inheritdoc />
        public void LoadProcess(IProcess process)
        {
            ProcessHandler.Initialize(process);
        }

        /// <inheritdoc />
        public void StartProcess()
        {
            ProcessHandler.StartProcess();
        }

        private void OnDisable()
        {
            ProcessHandler.StopProcess();
        }

        private void OnSelectedProcessChanged(string selectedProcess)
        {
            selectedProcessChanged?.Invoke(selectedProcess);
        }

        private static string GetProcessNameFromPath(string path)
        {
            int slashIndex = path.LastIndexOf('/');
            string fileName = path.Substring(slashIndex + 1);
            int pointIndex = fileName.LastIndexOf('.');
            fileName = fileName.Substring(0, pointIndex);

            return fileName;
        }

        private async Task<List<byte[]>> GetAdditionalProcessData(string processFolder, IProcessAssetManifest manifest)
        {
            List<byte[]> additionalData = new List<byte[]>();
            foreach (string fileName in manifest.AdditionalFileNames)
            {
                string filePath = $"{processFolder}/{fileName}.{Serializer.FileFormat}";

                if (await ServiceRegistry.Get<IPlatformFileSystem>().Exists(filePath))
                {
                    additionalData.Add(await ServiceRegistry.Get<IPlatformFileSystem>().Read(filePath));
                }
                else
                {
                    ForwardingLogger.Log($"Error loading process. File not found: {filePath}");
                }
            }

            return additionalData;
        }
    }
}
