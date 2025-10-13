// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.PackageManager.Editor;

namespace VRBuilder.Core.Editor
{
    /// <summary>
    /// Retrieves the current named build target based on the active build target settings in the Unity Editor.
    /// Utility helper to ease up working with Unity Editor.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorUtils
    {
        private const string ignoreEditorImguiTestsDefineSymbol = "BUILDER_IGNORE_EDITOR_IMGUI_TESTS";
        private const string corePackageName = "co.mindport.vrbuilder.core";

        private static string coreFolder;
        private static bool isUpmPackage = true;

        private static MethodInfo repaintImmediately = typeof(EditorWindow).GetMethod("RepaintImmediately", BindingFlags.Instance | BindingFlags.NonPublic, null, Array.Empty<Type>(), Array.Empty<ParameterModifier>());

        /// <summary>
        /// True if VR Builder is a Package Manager package.
        /// </summary>
        public static bool IsUpmPackage => isUpmPackage;

        static EditorUtils()
        {
            AssemblyReloadEvents.afterAssemblyReload += ResolveCoreFolder;
            EditorApplication.playModeStateChanged += ResolveCoreFolder;
        }

        private static void EnableEditorImguiTests()
        {
            SetImguiTestsState(true);
        }

        private static void DisableImguiTests()
        {
            SetImguiTestsState(false);
        }

        private static void SetImguiTestsState(bool enabled)
        {
            List<string> symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Split(';').ToList();

            bool wasEnabled = symbols.Contains(ignoreEditorImguiTestsDefineSymbol) == false;

            if (wasEnabled != enabled)
            {
                if (enabled)
                {
                    symbols.Remove(ignoreEditorImguiTestsDefineSymbol);
                }
                else
                {
                    symbols.Add(ignoreEditorImguiTestsDefineSymbol);
                }

                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(";", symbols.ToArray()));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            }
        }

        /// <summary>
        /// Returns true if there is a window of type <typeparamref name="T"/> opened.
        /// </summary>
        internal static bool IsWindowOpened<T>() where T : EditorWindow
        {
            // https://answers.unity.com/questions/523839/find-out-if-an-editor-window-is-open.html
            T[] windows = Resources.FindObjectsOfTypeAll<T>();
            return windows != null && windows.Length > 0;
        }

        /// <summary>
        /// Causes the target <paramref name="window"/> to repaint immediately. Used for testing.
        /// </summary>
        internal static void RepaintImmediately(this EditorWindow window)
        {
            repaintImmediately.Invoke(window, Array.Empty<object>());
        }

        /// <summary>
        /// Takes the focus away the field where you was typing something into.
        /// </summary>
        internal static void ResetKeyboardElementFocus()
        {
            GUIUtility.keyboardControl = 0;
        }

        /// <summary>
        /// Gets the root folder of VR Builder.
        /// </summary>
        public static string GetCoreFolder()
        {
            if (coreFolder == null)
            {
                ResolveCoreFolder();
            }

            return coreFolder;
        }

        /// <summary>
        /// Returns the Core version as string.
        /// </summary>
        internal static string GetCoreVersion()
        {
            string version = PackageOperationsManager.GetInstalledPackageVersion(corePackageName);
            return string.IsNullOrEmpty(version) ? "unknown" : version;
        }

        /// <summary>
        /// Retrieves the current named build target based on the active build target settings in the Unity Editor.
        /// </summary>
        /// <returns>
        /// A <see cref="NamedBuildTarget"/> object representing the current build target group.
        /// </returns>
        internal static NamedBuildTarget GetCurrentNamedBuildTarget()
        {
            BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);
            return NamedBuildTarget.FromBuildTargetGroup(targetGroup);
        }

        /// <summary>
        /// Gets .NET API compatibility level for current BuildTargetGroup.
        /// </summary>
        internal static ApiCompatibilityLevel GetCurrentCompatibilityLevel()
        {
            return PlayerSettings.GetApiCompatibilityLevel(GetCurrentNamedBuildTarget());
        }

        /// <summary>
        /// Returns a list of scriptable objects from provided type;
        /// </summary>
        internal static IEnumerable<T> GetAllScriptableObjects<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>);
        }


        /// <summary>
        ///  Make sure that all necessary VisualTreeAssets are set in the Inspector.
        /// </summary>
        /// <param name="source">Name of the editor class</param>
        /// <param name="asset">List of all assets</param>
        internal static void CheckVisualTreeAssets(string source, List<VisualTreeAsset> asset)
        {
            if (asset == null)
            {
                return;
            }
            foreach (VisualTreeAsset treeAsset in asset)
            {
                CheckVisualTreeAsset(source, treeAsset);
            }
        }

        /// <summary>
        /// Make sure that the VisualTreeAsset is set in the Inspector.
        /// </summary>
        /// <param name="source">Name of the editor class</param>
        internal static void CheckVisualTreeAsset(string source, VisualTreeAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException($"A VisualTreeAsset in {source} not assigned in the Inspector.");
            }
        }

        private static void ResolveCoreFolder(PlayModeStateChange state)
        {
            ResolveCoreFolder();
        }

        [DidReloadScripts]
        private static void ResolveCoreFolder()
        {
            string[] roots = Array.Empty<string>();
            string projectFolder = "";

            // Check Packages folder
            try
            {
                projectFolder = Application.dataPath.Replace("/Assets", "");
                string packagePath = $"/Packages/{corePackageName}";
                roots = Directory.GetFiles(projectFolder + packagePath, "package.json", SearchOption.AllDirectories);
            }
            catch (DirectoryNotFoundException)
            {
                isUpmPackage = false;
            }

            if (roots.Length == 0)
            {
                // Check Assets folder
                projectFolder = Application.dataPath;
                roots = Directory.GetFiles(projectFolder, "package.json", SearchOption.AllDirectories);
            }

            if (roots.Length == 0)
            {
                throw new FileNotFoundException("VR Builder Core folder not found!");
            }

            coreFolder = Path.GetDirectoryName(roots.First());

            coreFolder = coreFolder.Substring(projectFolder.Length);
            coreFolder = coreFolder.Substring(1, coreFolder.Length - 1);

            if (IsUpmPackage == false)
            {
                coreFolder = $"Assets\\{coreFolder}";
            }

            // Replace backslashes with forward slashes.
            coreFolder = coreFolder.Replace('/', Path.AltDirectorySeparatorChar);
        }
    }
}
