// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine.SceneManagement;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Entities.Factories;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Helper class to setup scenes and processes.
    /// </summary>
    internal class SceneSetupUtils
    {
        public const string SceneDirectory = "Assets/Scenes";

        /// <summary>
        /// Creates and saves a new scene with given <paramref name="sceneName"/>.
        /// </summary>
        /// <param name="sceneName">Name of the scene.</param>
        /// <param name="directory">Directory to save scene in.</param>
        public static void CreateNewScene(string sceneName, string directory = SceneDirectory, string templatePath = "")
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            if (string.IsNullOrEmpty(templatePath) == false)
            {
                SceneTemplateAsset templateAsset = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(templatePath);

                if (templateAsset != null)
                {
                    SceneTemplateService.Instantiate(templateAsset, false, $"{directory}/{sceneName}.unity");
                    return;
                }
                else
                {
                    UnityEngine.Debug.LogError($"Scene template not found at {templatePath}. Creating default scene.");
                    return;
                }
            }

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, $"{directory}/{sceneName}.unity");
            EditorSceneManager.OpenScene($"{directory}/{sceneName}.unity");
        }

        /// <summary>
        /// Returns true if provided <paramref name="sceneName"/> exits in given <paramref name="directory"/>.
        /// </summary>
        public static bool SceneExists(string sceneName, string directory = SceneDirectory)
        {
            return File.Exists($"{directory}/{sceneName}.unity");
        }

        /// <summary>
        /// Creates a new process for this scene.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        public static void SetupProcess(string processName)
        {
            string errorMessage = null;
            if (ProcessAssetUtils.DoesProcessAssetExist(processName) || ProcessAssetUtils.CanCreate(processName, out errorMessage))
            {
                if (ProcessAssetUtils.DoesProcessAssetExist(processName))
                {
                    ProcessAssetManager.Load(processName);
                }
                else
                {
                    ProcessAssetManager.Import(EntityFactory.CreateProcess(processName));
                    AssetDatabase.Refresh();
                }

                SetProcessInCurrentScene(processName);
            }

            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                UnityEngine.Debug.LogError(errorMessage);
            }

            try
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Sets the process with given <paramref name="processName"/> for the current scene.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        public static void SetProcessInCurrentScene(string processName)
        {
            RuntimeConfigurator.Instance.SetSelectedProcess(ProcessAssetUtils.GetProcessStreamingAssetPath(processName));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            GlobalEditorHandler.SetCurrentProcess(processName);
            GlobalEditorHandler.StartEditingProcess();
        }
    }
}
