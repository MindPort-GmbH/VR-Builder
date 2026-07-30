using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core.IO;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Serialization;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Utils.Logging;

namespace VRBuilder.Core.Configuration
{
    public class RuntimeService : IRuntimeService
    {
        private IRuntimeServiceConfiguration configuration;
        private IRuntimeConfigurator? configurator;
        private string selectedProcessStreamingAssetsPath;

        public string SelectedProcess
        {
            get => configurator.RuntimeConfiguration.SelectedProcess;
            set => configurator.RuntimeConfiguration.SelectedProcess = value;
        }

        public string SelectedProcessStreamingAssetsPath
        {
            get => configuration.SelectedProcessStreamingAssetsPath;
            set => configuration.SelectedProcessStreamingAssetsPath = value;
        }

        /// <inheritdoc />
        public IProcessSerializer Serializer { get; set; } = new NewtonsoftJsonProcessSerializerV4();

        public Action<string?> selectedProcessChanged;

        public event Action<string?> SelectedProcessChanged
        {
            add => selectedProcessChanged += value;
            remove => selectedProcessChanged -= value;
        }

        /// <summary>
        /// Name of the manifest file that could be used to save process asset information.
        /// </summary>
        public string ManifestFileName
        {
            get => configurator.RuntimeConfiguration.ManifestFileName;
            set => configurator.RuntimeConfiguration.ManifestFileName = value;
        }

        public IRuntimeConfigurator Configurator { get; set; }
        public ILifeCycleLoggingConfiguration LifeCycleLogging => LifeCycleLoggingConfig.Instance;

        private static string GetProcessNameFromPath(string path)
        {
            int slashIndex = path.LastIndexOf('/');
            string fileName = path.Substring(slashIndex + 1);
            int pointIndex = fileName.LastIndexOf('.');
            fileName = fileName.Substring(0, pointIndex);

            return fileName;
        }


        public async Task<IProcess> LoadProcess(string path)
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

        public void SetConfiguration(IRuntimeServiceConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}