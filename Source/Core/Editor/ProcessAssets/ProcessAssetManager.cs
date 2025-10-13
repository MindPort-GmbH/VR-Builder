// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.IO;
using VRBuilder.Core.Serialization;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.ProcessAssets
{
    /// <summary>
    /// A static class that handles the process assets. It lets you to save, load, delete, and import processes and provides multiple related utility methods.
    /// </summary>
    public static class ProcessAssetManager
    {
        private static FileSystemWatcher watcher;
        private static bool isSaving;
        private static object lockObject = new object();

        /// <summary>
        /// Called when an external change to the process file is detected.
        /// </summary>
        public static event EventHandler ExternalFileChange;

        /// <summary>
        /// Deletes the process with <paramref name="processName"/>.
        /// </summary>
        public static void Delete(string processName)
        {
            if (ProcessAssetUtils.DoesProcessAssetExist(processName))
            {
                Directory.Delete(ProcessAssetUtils.GetProcessAssetDirectory(processName), true);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Imports the given <paramref name="process"/> by saving it to the proper directory. If there is a name collision, this process will be renamed.
        /// </summary>
        public static void Import(IProcess process)
        {
            int counter = 0;
            string oldName = process.Data.Name;
            while (ProcessAssetUtils.DoesProcessAssetExist(process.Data.Name))
            {
                if (counter > 0)
                {
                    process.Data.SetName(process.Data.Name.Substring(0, process.Data.Name.Length - 2));
                }

                counter++;
                process.Data.SetName(process.Data.Name + " " + counter);
            }

            if (oldName != process.Data.Name)
            {
                UnityEngine.Debug.LogWarning($"We detected a name collision while importing process \"{oldName}\". We have renamed it to \"{process.Data.Name}\" before importing.");
            }

            Save(process);
        }

        /// <summary>
        /// Imports the process from file at given file <paramref name="path"/> if the file extensions matches the <paramref name="serializer"/>.
        /// </summary>
        public static void Import(string path, IProcessSerializer serializer)
        {
            IProcess process;

            if (Path.GetExtension(path) != $".{serializer.FileFormat}")
            {
                UnityEngine.Debug.LogError($"The file extension of {path} does not match the expected file extension of {serializer.FileFormat} of the current serializer.");
            }

            try
            {
                byte[] file = File.ReadAllBytes(path);
                process = serializer.ProcessFromByteArray(file);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"{e.GetType().Name} occured while trying to import file '{path}' with serializer '{serializer.GetType().Name}'\n\n{e.StackTrace}");
                return;
            }

            Import(process);
        }

        /// <summary>
        /// Save the <paramref name="process"/> to the file system.
        /// </summary>
        public static void Save(IProcess process)
        {
            try
            {
                IDictionary<string, byte[]> assetData = EditorConfigurator.Instance.ProcessAssetStrategy.CreateSerializedProcessAssets(process, EditorConfigurator.Instance.Serializer);
                List<string> filesToDelete = new List<string>();
                string processDirectory = ProcessAssetUtils.GetProcessAssetDirectory(process.Data.Name);

                if (Directory.Exists(processDirectory))
                {
                    filesToDelete.AddRange(Directory.GetFiles(processDirectory, $"*.{EditorConfigurator.Instance.Serializer.FileFormat}"));
                }

                foreach (string fileName in assetData.Keys)
                {
                    string fullFileName = $"{fileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";
                    filesToDelete.Remove(filesToDelete.FirstOrDefault(file => file.EndsWith(fullFileName)));
                    string path = $"{processDirectory}/{fullFileName}";

                    WriteFileIfChanged(assetData[fileName], path);
                }

                if (EditorConfigurator.Instance.ProcessAssetStrategy.CreateManifest)
                {
                    byte[] manifestData = CreateSerializedManifest(assetData);
                    string fullManifestName = $"{BaseRuntimeConfiguration.ManifestFileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";
                    string manifestPath = $"{processDirectory}/{fullManifestName}";

                    WriteFileIfChanged(manifestData, manifestPath);

                    filesToDelete.Remove(filesToDelete.FirstOrDefault(file => file.EndsWith($"{fullManifestName}")));
                }

                DeleteFiles(filesToDelete);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        private static void DeleteFiles(IEnumerable<string> filesToDelete)
        {
            foreach (string file in filesToDelete)
            {
                UnityEngine.Debug.Log($"File deleted: {file}");
                File.Delete(file);
            }
        }

        private static byte[] CreateSerializedManifest(IDictionary<string, byte[]> assetData)
        {
            IProcessAssetManifest manifest = new ProcessAssetManifest()
            {
                AssetStrategyTypeName = EditorConfigurator.Instance.ProcessAssetStrategy.GetType().FullName,
                AdditionalFileNames = assetData.Keys.Where(name => name != assetData.Keys.First()).ToList(),
                ProcessFileName = assetData.Keys.First(),
            };

            byte[] manifestData = EditorConfigurator.Instance.Serializer.ManifestToByteArray(manifest);
            return manifestData;
        }

        private static void WriteFileIfChanged(byte[] data, string path)
        {
            byte[] storedData = Array.Empty<byte>();

            if (File.Exists(path))
            {
                storedData = File.ReadAllBytes(path);
            }

            if (Enumerable.SequenceEqual(storedData, data) == false)
            {
                if (AssetDatabase.MakeEditable(path))
                {
                    WriteProcessFile(path, data);
                    UnityEngine.Debug.Log($"File saved: \"{path}\"");
                }
                else
                {
                    UnityEngine.Debug.LogError($"Saving of \"{path}\" failed! Could not make it editable.");
                }
            }
        }

        private static void WriteProcessFile(string path, byte[] processData)
        {
            lock (lockObject)
            {
                isSaving = true;
            }

            FileStream stream = null;
            try
            {
                if (File.Exists(path))
                    File.SetAttributes(path, FileAttributes.Normal);

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                stream = File.Create(path);
                stream.Write(processData, 0, processData.Length);
                stream.Close();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Loads the process with the given <paramref name="processName"/> from the file system and converts it into the <seealso cref="IProcess"/> instance.
        /// Sets up a file system watcher to monitor changes in the directory of the process.
        /// </summary>
        /// <param name="processName">The name of the process to load or <seealso cref="string.Empty"/> if the scene dos not contain a process.</param>
        public static IProcess Load(string processName)
        {
            if (ProcessAssetUtils.DoesProcessAssetExist(processName))
            {
                string manifestPath = $"{ProcessAssetUtils.GetProcessAssetDirectory(processName)}/{BaseRuntimeConfiguration.ManifestFileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";

                IProcessAssetManifest manifest = CreateProcessManifest(processName, manifestPath);
                IProcessAssetStrategy assetStrategy = ReflectionUtils.CreateInstanceOfType(ReflectionUtils.GetConcreteImplementationsOf<IProcessAssetStrategy>().FirstOrDefault(type => type.FullName == manifest.AssetStrategyTypeName)) as IProcessAssetStrategy;
                List<byte[]> additionalData = LoadAdditionalDataFromManifest(processName, manifest);

                string processAssetPath = ProcessAssetUtils.GetProcessAssetPath(processName);
                byte[] processData = File.ReadAllBytes(processAssetPath);

                SetupWatcher(processName);

                try
                {
                    return assetStrategy.GetProcessFromSerializedData(processData, additionalData, EditorConfigurator.Instance.Serializer);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to load the process '{processName}' from '{processAssetPath}' because of: \n{ex.Message}");
                    UnityEngine.Debug.LogError(ex);
                }
            }
            else
            {
                DisposeWatcher();
            }
            return null;
        }

        /// <summary>
        /// Renames the <paramref name="process"/> to the <paramref name="newName"/> and moves it to the appropriate directory. Check if you can rename before with the <seealso cref="CanRename"/> method.
        /// </summary>
        public static void RenameProcess(IProcess process, string newName)
        {
            if (ProcessAssetUtils.CanRename(process, newName, out string errorMessage) == false)
            {
                UnityEngine.Debug.LogError($"Process {process.Data.Name} was not renamed because:\n\n{errorMessage}");
                return;
            }

            string oldDirectory = ProcessAssetUtils.GetProcessAssetDirectory(process.Data.Name);
            string newDirectory = ProcessAssetUtils.GetProcessAssetDirectory(newName);

            Directory.Move(oldDirectory, newDirectory);
            File.Move($"{oldDirectory}.meta", $"{newDirectory}.meta");

            string newAsset = ProcessAssetUtils.GetProcessAssetPath(newName);
            string oldAsset = $"{ProcessAssetUtils.GetProcessAssetDirectory(newName)}/{process.Data.Name}.{EditorConfigurator.Instance.Serializer.FileFormat}";
            File.Move(oldAsset, newAsset);
            File.Move($"{oldAsset}.meta", $"{newAsset}.meta");
            process.Data.SetName(newName);

            Save(process);

            string streamingAssetPath = ProcessAssetUtils.GetProcessStreamingAssetPath(process.Data.Name);

            if (newAsset.EndsWith(streamingAssetPath) == false)
            {
                UnityEngine.Debug.LogError($"Process {process.Data.Name} is stored in an invalid path.");
            }

            RuntimeConfigurator.Instance.SetSelectedProcess(streamingAssetPath);
        }

        /// <summary>
        /// Creates a new <seealso cref="IProcessAssetManifest"/> for the given <paramref name="processName"/> and <paramref name="manifestPath"/>.
        /// If a <paramref name="manifestPath"/> file does not exist the process was saved with the <seealso cref="SingleFileProcessAssetStrategy"/>.
        /// This strategy does not include a manifest file.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <param name="manifestPath">The path including filename to a manifest file.</param>
        /// <returns></returns>
        private static IProcessAssetManifest CreateProcessManifest(string processName, string manifestPath)
        {
            IProcessAssetManifest manifest;
            if (File.Exists(manifestPath))
            {
                byte[] manifestData = File.ReadAllBytes(manifestPath);
                manifest = EditorConfigurator.Instance.Serializer.ManifestFromByteArray(manifestData);
            }
            else
            {
                manifest = new ProcessAssetManifest()
                {
                    AssetStrategyTypeName = typeof(SingleFileProcessAssetStrategy).FullName,
                    AdditionalFileNames = new List<string>(),
                    ProcessFileName = processName,
                };
            }

            return manifest;
        }

        /// <summary>
        /// Loads all data from the files inside <see cref="IProcessAssetManifest.AdditionalFileNames"/> and returns it.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <param name="manifest">The manifest object.</param>
        /// <returns>A list of byte arrays representing additional data.</returns>
        private static List<byte[]> LoadAdditionalDataFromManifest(string processName, IProcessAssetManifest manifest)
        {
            List<byte[]> additionalData = new List<byte[]>();
            foreach (string fileName in manifest.AdditionalFileNames)
            {
                string path = $"{ProcessAssetUtils.GetProcessAssetDirectory(processName)}/{fileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";

                if (File.Exists(path))
                {
                    additionalData.Add(File.ReadAllBytes(path));
                }
                else
                {
                    UnityEngine.Debug.Log($"Error loading process. File not found: {path}");
                }
            }

            return additionalData;
        }


        /// <summary>
        /// Sets up a file system watcher to monitor changes to all files of type <see cref="EditorConfigurator.Instance.Serializer.FileFormat"/> 
        /// in the specified process asset directory. If the path changes, we dispose the existing watcher and create a new one.
        /// </summary>
        /// <param name="processName">The name of the new process to be watched.</param>
        /// <remarks>
        /// We need to dispose the watcher and create a new one if the path changes. If we don't do this, the watcher will listen to the old path.
        /// This seems to be a bug in the Unity implementation of FileSystemWatcher (Tested with Unity 2022.3.25).
        /// </remarks>
        private static void SetupWatcher(string processName)
        {
            if (watcher != null)
            {
                if (watcher.Path == ProcessAssetUtils.GetProcessAssetDirectory(processName))
                {
                    return;
                }
                DisposeWatcher();
            }

            watcher = new FileSystemWatcher();
            watcher.Changed += OnFileChanged;
            watcher.Path = ProcessAssetUtils.GetProcessAssetDirectory(processName);
            watcher.Filter = $"*.{EditorConfigurator.Instance.Serializer.FileFormat}";
        }

        private static void DisposeWatcher()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (isSaving)
            {
                lock (lockObject)
                {
                    isSaving = false;
                }
                return;
            }


            ExternalFileChange?.Invoke(null, EventArgs.Empty);
        }
    }
}
