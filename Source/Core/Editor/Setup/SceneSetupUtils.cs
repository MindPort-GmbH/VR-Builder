// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System;
using System.IO;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace VRBuilder.Editor.Setup
{
    /// <summary>
    /// Helper class to setup scenes and processes.
    /// </summary>
    internal class SceneSetupUtils
    {
        public const string SceneDirectory = "Assets/Scenes";
        private const string SimpleExampleName = "Hello Creator - A 5-step Guide";

        /// <summary>
        /// Creates and saves a new scene with given <paramref name="sceneName"/>.
        /// </summary>
        /// <param name="sceneName">Name of the scene.</param>
        /// <param name="directory">Directory to save scene in.</param>
        public static void CreateNewScene(string sceneName, string directory = SceneDirectory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
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
        /// Sets up the current scene and creates a new process for this scene.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        public static void SetupSceneAndProcess(string processName)
        {
            ProcessSceneSetup.Run();

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
                Debug.LogError(errorMessage);
            }

            try
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
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

        /// <summary>
        /// Creates and saves a new simple example scene.
        /// </summary>
        /// <remarks>The new scene is meant to be used for step by step guides.</remarks>
        public static void CreateNewSimpleExampleScene()
        {
            string processName = SimpleExampleName;
            int counter = 1;

            while (ProcessAssetUtils.DoesProcessAssetExist(processName) || ProcessAssetUtils.CanCreate(processName, out string errorMessage) == false)
            {
                processName = $"{SimpleExampleName}_{counter}";
                counter++;
            }

            CreateNewScene(processName);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Sphere";
            sphere.transform.position = new Vector3(0f, 0.5f, 2f);

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Plane";
            plane.transform.localScale = new Vector3(2f, 2f, 2f);

            SetupSceneAndProcess(processName);
        }
    }
}
