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
    /// Default runtime service
    /// </summary>
    public class RuntimeService : IRuntimeService
    {
        private Action<string?> selectedProcessChanged;

        private IRuntimeServiceConfiguration configuration;
        private IRuntimeHandler? handler;
        private IConfigurableProcessHandler? processHandler;
        private string selectedProcessStreamingAssetsPath;

        public event Action<string?> SelectedProcessChanged
        {
            add => selectedProcessChanged += value;
            remove => selectedProcessChanged -= value;
        }

        public IRuntimeHandler Handler
        {
            get => handler;
            set
            {
                string previousSelection = SelectedProcess;

                if (handler is RuntimeHandler previousConfigurator)
                {
                    previousConfigurator.SelectedProcessChanged -= OnSelectedProcessChanged;
                }

                handler = value;

                if (handler is RuntimeHandler currentConfigurator)
                {
                    currentConfigurator.SelectedProcessChanged += OnSelectedProcessChanged;
                }

                string currentSelection = handler?.RuntimeConfiguration?.SelectedProcess ?? string.Empty;
                if (string.Equals(previousSelection, currentSelection, StringComparison.Ordinal) == false)
                {
                    selectedProcessChanged?.Invoke(currentSelection);
                }
            }
        }

        public string SelectedProcess
        {
            get => handler?.RuntimeConfiguration?.SelectedProcess ?? string.Empty;
            set
            {
                if (handler == null || (handler.RuntimeConfiguration?.SelectedProcess ?? string.Empty) == (value ?? string.Empty))
                {
					return;
                }

				handler.RuntimeConfiguration.SelectedProcess = value;

                // If the configurator forwards configuration changes through its own event, the chain
                // (RuntimeConfiguration -> RuntimeConfigurator -> RuntimeService) already raises this event.
                if (handler is not RuntimeHandler { RuntimeConfiguration: RuntimeConfiguration })
                {
                    selectedProcessChanged?.Invoke(value);
                }
            }
        }

        public IConfigurableProcessHandler ProcessHandler
        {
            get => processHandler;
            set => processHandler = value;
        }

        public string SelectedProcessStreamingAssetsPath
        {
            get => configuration.SelectedProcessStreamingAssetsPath;
            set => configuration.SelectedProcessStreamingAssetsPath = value;
        }

        /// <inheritdoc />
        public IProcessSerializer Serializer { get; set; } = new NewtonsoftJsonProcessSerializerV4();

        /// <summary>
        /// Name of the manifest file that could be used to save process asset information.
        /// </summary>
        public string ManifestFileName
        {
            get => handler?.RuntimeConfiguration?.ManifestFileName ?? "ProcessManifest";
            set
            {
                if (handler != null)
                {
                    handler.RuntimeConfiguration.ManifestFileName = value;
                }
            }
        }

        public void SetConfiguration(IRuntimeServiceConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private void OnSelectedProcessChanged(string selectedProcess)
        {
            selectedProcessChanged?.Invoke(selectedProcess);
        }

        public ILifeCycleLoggingConfiguration LifeCycleLogging => LifeCycleLoggingConfig.Instance;

        private static string GetProcessNameFromPath(string path)
        {
            int slashIndex = path.LastIndexOf('/');
            string fileName = path.Substring(slashIndex + 1);
            int pointIndex = fileName.LastIndexOf('.');
            fileName = fileName.Substring(0, pointIndex);

            return fileName;
        }

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

                int index = path.LastIndexOf("/");
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

        public void LoadProcess(IProcess process)
        {
            ProcessHandler.Initialize(process);
        }

        public void StartProcess()
        {
            ProcessHandler.StartProcess();
        }

        private void OnDisable()
        {
            ProcessHandler.StopProcess();
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
