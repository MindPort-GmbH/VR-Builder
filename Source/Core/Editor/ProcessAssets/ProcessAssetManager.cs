// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.IO;
using VRBuilder.Core;
using VRBuilder.Core.Serialization;
using VRBuilder.Editor.Configuration;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Configuration;
using System.Linq;
using VRBuilder.Core.IO;
using System.Collections.Generic;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor
{
    /// <summary>
    /// A static class that handles the process assets. It lets you to save, load, delete, and import processes and provides multiple related utility methods.
    /// </summary>
    internal static class ProcessAssetManager
    {
        private static FileSystemWatcher watcher;
        private static bool isSaving;
        private static object lockObject = new object();

        /// <summary>
        /// Called when an external change to the process file is detected.
        /// </summary>
        internal static event EventHandler ExternalFileChange;

        /// <summary>
        /// Deletes the process with <paramref name="processName"/>.
        /// </summary>
        internal static void Delete(string processName)
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
        internal static void Import(IProcess process)
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
                Debug.LogWarning($"We detected a name collision while importing process \"{oldName}\". We have renamed it to \"{process.Data.Name}\" before importing.");
            }

            Save(process);
        }

        /// <summary>
        /// Imports the process from file at given file <paramref name="path"/> if the file extensions matches the <paramref name="serializer"/>.
        /// </summary>
        internal static void Import(string path, IProcessSerializer serializer)
        {
            IProcess process;

            if (Path.GetExtension(path) != $".{serializer.FileFormat}")
            {
                Debug.LogError($"The file extension of {path} does not match the expected file extension of {serializer.FileFormat} of the current serializer.");
            }

            try
            {
                byte[] file = File.ReadAllBytes(path);
                process = serializer.ProcessFromByteArray(file);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.GetType().Name} occured while trying to import file '{path}' with serializer '{serializer.GetType().Name}'\n\n{e.StackTrace}");
                return;
            }

            Import(process);
        }

        /// <summary>
        /// Save the <paramref name="process"/> to the file system.
        /// </summary>
        internal static void Save(IProcess process)
        {
            try
            {
                IDictionary<string, byte[]> assetData = EditorConfigurator.Instance.ProcessAssetStrategy.CreateSerializedProcessAssets(process, EditorConfigurator.Instance.Serializer);
                List<string> filesInFolder = Directory.GetFiles(ProcessAssetUtils.GetProcessAssetDirectory(process.Data.Name), $"*.{EditorConfigurator.Instance.Serializer.FileFormat}").ToList();

                foreach (string fileName in assetData.Keys)
                {
                    filesInFolder.Remove(filesInFolder.FirstOrDefault(file => file.EndsWith($"{fileName}.{EditorConfigurator.Instance.Serializer.FileFormat}")));

                    byte[] storedData = new byte[0];
                    string path = $"{ProcessAssetUtils.GetProcessAssetDirectory(process.Data.Name)}/{fileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";                  

                    if (File.Exists(path))
                    {
                        storedData = File.ReadAllBytes(path);
                    }

                    if (Enumerable.SequenceEqual(storedData, assetData[fileName]) == false)
                    {
                        AssetDatabase.MakeEditable(path);
                        WriteProcessFile(path, assetData[fileName]);
                        Debug.Log($"File saved: \"{path}\"");
                    }
                }

                if (EditorConfigurator.Instance.ProcessAssetStrategy.CreateManifest)
                {
                    IProcessAssetManifest manifest = new ProcessAssetManifest()
                    {
                        AssetStrategyTypeName = EditorConfigurator.Instance.ProcessAssetStrategy.GetType().FullName,
                        AdditionalFileNames = assetData.Keys.Where(name => name != assetData.Keys.First()).ToList(),
                        ProcessFileName = assetData.Keys.First(),
                    };

                    byte[] manifestData = EditorConfigurator.Instance.Serializer.ManifestToByteArray(manifest);

                    string manifestPath = $"{ProcessAssetUtils.GetProcessAssetDirectory(process.Data.Name)}/{BaseRuntimeConfiguration.ManifestFileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";

                    if (File.Exists(manifestPath))
                    {
                        byte[] storedManifestData = File.ReadAllBytes(manifestPath);

                        if (Enumerable.SequenceEqual(storedManifestData, manifestData) == false)
                        {
                            AssetDatabase.MakeEditable(manifestPath);
                            WriteProcessFile(manifestPath, manifestData);
                        }
                    }
                    else
                    {
                        WriteProcessFile(manifestPath, manifestData);
                    }

                    filesInFolder.Remove(filesInFolder.FirstOrDefault(file => file.EndsWith($"{BaseRuntimeConfiguration.ManifestFileName}.{EditorConfigurator.Instance.Serializer.FileFormat}")));
                }

                foreach (string file in filesInFolder)
                {
                    Debug.Log($"File deleted: {file}");
                    File.Delete(file);
                }

            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static void WriteProcessFile(string path, byte[] processData)
        {
            lock(lockObject)
            {
                isSaving = true;
            }

            FileStream stream = null;
            try
            {
                if(File.Exists(path))
                   File.SetAttributes(path, FileAttributes.Normal);

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                stream = File.Create(path);
                stream.Write(processData, 0, processData.Length);
                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
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
        /// </summary>
        internal static IProcess Load(string processName)
        {
            if (ProcessAssetUtils.DoesProcessAssetExist(processName))
            {
                string manifestPath = $"{ProcessAssetUtils.GetProcessAssetDirectory(processName)}/{BaseRuntimeConfiguration.ManifestFileName}.{EditorConfigurator.Instance.Serializer.FileFormat}";

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

                IProcessAssetStrategy assetStrategy = ReflectionUtils.CreateInstanceOfType(ReflectionUtils.GetConcreteImplementationsOf<IProcessAssetStrategy>().FirstOrDefault(type => type.FullName == manifest.AssetStrategyTypeName)) as IProcessAssetStrategy;

                string processAssetPath = ProcessAssetUtils.GetProcessAssetPath(processName);
                byte[] processData = File.ReadAllBytes(processAssetPath);

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
                        Debug.Log($"Error loading process. File not found: {path}");
                    }
                }

                SetupWatcher(processName);

                try
                {
                    return assetStrategy.GetProcessFromSerializedData(processData, additionalData, EditorConfigurator.Instance.Serializer);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load the process '{processName}' from '{processAssetPath}' because of: \n{ex.Message}");
                    Debug.LogError(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Renames the <paramref name="process"/> to the <paramref name="newName"/> and moves it to the appropriate directory. Check if you can rename before with the <seealso cref="CanRename"/> method.
        /// </summary>
        internal static void RenameProcess(IProcess process, string newName)
        {
            if (ProcessAssetUtils.CanRename(process, newName, out string errorMessage) == false)
            {
                Debug.LogError($"Process {process.Data.Name} was not renamed because:\n\n{errorMessage}");
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

            RuntimeConfigurator.Instance.SetSelectedProcess(newAsset);
        }

        private static void SetupWatcher(string processName)
        {
            if (watcher == null)
            {
                watcher = new FileSystemWatcher();
                watcher.Changed += OnFileChanged;
            }

            watcher.Path = ProcessAssetUtils.GetProcessAssetDirectory(processName);
            watcher.Filter = $"*.{EditorConfigurator.Instance.Serializer.FileFormat}";            

            watcher.EnableRaisingEvents = true;            
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {            
            if(isSaving)
            {
                lock(lockObject)
                {
                    isSaving = false;
                }
                return;
            }

            ExternalFileChange?.Invoke(null, EventArgs.Empty);
        }
    }
}
