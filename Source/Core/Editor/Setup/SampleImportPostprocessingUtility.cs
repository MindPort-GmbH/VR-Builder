using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VRBuilder.Core.Editor.Setup
{
    /// <summary>
    /// Reusable helpers for sample import postprocessing.
    /// </summary>
    public static class SampleImportPostprocessingUtility
    {
        private const string SampleImportedFlagFileName = "ImportSamplePostProcessingFlag.md";
        private const string SourceStreamingAssetsFolderName = "StreamingAssets~/Processes";
        private const string ProjectStreamingAssetsRoot = "Assets/StreamingAssets/Processes";
        private const string OpenSceneAfterReloadFlagKey = "VRB.SampleImport.OpenSceneAfterReload.Flag";
        private const string OpenSceneAfterReloadNameKey = "VRB.SampleImport.OpenSceneAfterReload.SceneName";
        private const string OpenSceneAfterReloadsSampleRootPathKey = "VRB.SampleImport.OpenSceneAfterReload.SampleRootPrefix";

        /// <summary>
        /// Checks the presence of a sample imported flag file in the sample root directory.
        /// </summary>
        /// <param name="sampleRoot">The full path to the root directory of the sample.</param>
        /// <returns>
        /// <c>true</c> if there is a file present (indicating the import not should occur); otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSampleImportedFlagSet(string sampleRoot)
        {
            string markerPath = Path.Combine(sampleRoot, SampleImportedFlagFileName);
            if (File.Exists(markerPath))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates or overwrites a process copied flag file that indicates the copy step was completed.
        /// </summary>
        /// <param name="sampleName">The name of the sample being imported.</param>
        /// <param name="sampleRoot">The root directory where the sample resides and where the flag file will be created.</param>
        /// <returns>
        /// Returns <c>true</c> if the flag file was successfully created and imported into the AssetDatabase; otherwise, <c>false</c> if an exception occurred during the process.
        /// </returns>
        public static bool TryCreateSampleImportedFlag(string sampleName, string sampleRoot)
        {
            string markerPath = Path.Combine(sampleRoot, SampleImportedFlagFileName);
            try
            {
                string content = $"This file indicates that the sample '{sampleName}' was setup on {DateTime.Now:yyyy-MM-dd HH:mm:ss}.";
                File.WriteAllText(markerPath, content);
                AssetDatabase.ImportAsset(markerPath);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[VR Builder Sample Import] Failed to write sample imported flag file at '{markerPath}': {ex}");
                return false;
            }
        }

        public static void InitiateImportPostprocessing(string sampleRootPath, string sampleName, string processFileName, string demoSceneName, Action fixValidationIssues)
        {
            bool success = CopyProcessFile(sampleRootPath, sampleName, processFileName);
            TryCreateSampleImportedFlag(sampleName, sampleRootPath);
            if (success)
            {
                SessionState.SetBool(OpenSceneAfterReloadFlagKey, true);
                SessionState.SetString(OpenSceneAfterReloadNameKey, demoSceneName);
                SessionState.SetString(OpenSceneAfterReloadsSampleRootPathKey, sampleRootPath);


                FixAllIssuesSafely(fixValidationIssues, () => OpenSampleSceneSafely(demoSceneName, sampleRootPath));
            }
        }

        public static void FixAllIssuesSafely(Action fixValidationIssues, Action openOpenSampleSceneSafely)
        {
            if (AssetDatabase.IsAssetImportWorkerProcess() ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += () => FixAllIssuesSafely(fixValidationIssues, openOpenSampleSceneSafely);
                //UnityEngine.Debug.Log("[VR Builder - Sample Import] Delaying project validation until the editor is in a stable state.");
                return;
            }

            UnityEngine.Debug.Log($"[VR Builder - Sample Import] Running validation tasks and adding missing dependencies.");
            fixValidationIssues();

            // Best-effort immediate open; the after-reload hook will cover the reload case.
            EditorApplication.delayCall += () => openOpenSampleSceneSafely();
        }

        public static void OpenSampleSceneSafely(string demoSceneName, string sampleRootPath)
        {
            if (AssetDatabase.IsAssetImportWorkerProcess() ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // UnityEngine.Debug.Log($"[VR Builder - Sample Import] Delaying opening '{demoSceneName}' until the editor is in a stable state.");
                EditorApplication.delayCall += () => OpenSampleSceneSafely(demoSceneName, sampleRootPath);
                return;
            }

            UnityEngine.Debug.Log($"[VR Builder - Sample Import] Opening the demo scene '{demoSceneName}'.");
            OpenSampleScene(demoSceneName, sampleRootPath);

            SessionState.SetBool(OpenSceneAfterReloadFlagKey, false);
            SessionState.SetString(OpenSceneAfterReloadNameKey, string.Empty);
            SessionState.SetString(OpenSceneAfterReloadsSampleRootPathKey, string.Empty);
        }

        [InitializeOnLoadMethod]
        private static void AfterDomainReload()
        {
            bool shouldOpen = SessionState.GetBool(OpenSceneAfterReloadFlagKey, false);
            if (!shouldOpen)
            {
                return;
            }

            string sceneName = SessionState.GetString(OpenSceneAfterReloadNameKey, string.Empty);
            string samplesRootPrefix = SessionState.GetString(OpenSceneAfterReloadsSampleRootPathKey, string.Empty);
            if (string.IsNullOrEmpty(sceneName))
            {
                // Nothing to do; clear the flag defensively.
                SessionState.SetBool(OpenSceneAfterReloadFlagKey, false);
                return;
            }

            // Clear first to avoid loops, then schedule the safe open.
            SessionState.SetBool(OpenSceneAfterReloadFlagKey, false);
            SessionState.SetString(OpenSceneAfterReloadsSampleRootPathKey, string.Empty);

            EditorApplication.delayCall += () => OpenSampleSceneSafely(sceneName, samplesRootPrefix);
        }

        /// <summary>
        /// Executes the copy/compare/marker workflow for a single sample.
        /// </summary>
        /// <param name="sampleRootPath">Absolute asset path to the sample root folder.</param>
        public static bool CopyProcessFile(string sampleRootPath, string sampleName, string processFileName)
        {
            try
            {
                // Build source path inside the sample's /Assets/Samples/VR Builder/<version> + /StreamingAssets/Processes + /<DemoName> + /<File.json>
                string sourceJsonPath = Path.Combine(
                    Path.Combine(sampleRootPath, SourceStreamingAssetsFolderName),
                    Path.Combine(sampleName, processFileName));

                if (!File.Exists(sourceJsonPath))
                {
                    UnityEngine.Debug.LogError($"[VR Builder - Sample Import] Source JSON for '{sampleName}' not found at '{sourceJsonPath}'. Nothing to copy.");
                    return false;
                }

                // Prepare destination directory: Assets/StreamingAssets/Processes/<DemoName>/
                string targetDir = Path.Combine(ProjectStreamingAssetsRoot, sampleName);
                string destinationJsonPath = Path.Combine(targetDir, processFileName);

                bool copied = TryCopyFile(sourceJsonPath, destinationJsonPath, out string copyError);
                if (copied)
                {
                    UnityEngine.Debug.Log($"[VR Builder - Sample Import] Copied process JSON to '{destinationJsonPath}'.");
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[VR Builder - Sample Import] Failed to copy process JSON of '{sampleName}' from '{sourceJsonPath}' to '{destinationJsonPath}'. Error: {copyError}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[VR Builder - Sample Import] Exception while copying process JSON of '{sampleName} from '{sampleRootPath}': {ex}");
                return false;
            }
        }

        /// <summary>
        /// From a list of changed asset paths, collects unique sample roots that match:
        /// Assets/Samples/VR Builder/<version>/<sampleDisplayName>/...
        /// </summary>
        /// <param name="changedPaths">Changed asset paths (imported or moved).</param>
        /// <param name="sampleName">Sample folder name, e.g., 'Hands Interaction Demo'.</param>
        /// <param name="output">Destination list to which unique roots will be added.</param>
        /// <param name="samplesRootPrefix">The fixed first part of the path to the sample folder e.g. 'Assets/Samples/VR Builder'.</param>
        public static void CollectSampleRootsFromChanges(string[] changedPaths, string sampleName, List<string> output, string samplesRootPrefix)
        {
            if (changedPaths == null || changedPaths.Length == 0)
            {
                return;
            }

            for (int i = 0; i < changedPaths.Length; i++)
            {
                string p = changedPaths[i];
                if (string.IsNullOrEmpty(p))
                {
                    continue;
                }

                string normalized = p.Replace('\\', '/');
                if (!normalized.StartsWith(samplesRootPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string root = TryResolveSampleRootFromPath(normalized, samplesRootPrefix, sampleName);
                if (!string.IsNullOrEmpty(root) && !output.Contains(root, StringComparer.OrdinalIgnoreCase))
                {
                    output.Add(root);
                }
            }
        }

        /// <summary>
        /// Given a path under Assets/Samples/VR Builder/<version>/<sampleDisplayName>/...,
        /// returns the root path 'Assets/Samples/VR Builder/<version>/<sampleDisplayName>'.
        /// </summary>
        /// <param name="anyPathUnderSample">Any child path within the sample.</param>
        /// <param name="samplesRootPrefix">Prefix like 'Assets/Samples/VR Builder'.</param>
        /// <param name="sampleName">Sample folder name to anchor.</param>
        /// <returns>The resolved sample root path or empty string if not matched.</returns>
        public static string TryResolveSampleRootFromPath(string anyPathUnderSample, string samplesRootPrefix, string sampleName)
        {
            // Split into segments and locate the index of the sampleDisplayName.
            string[] parts = anyPathUnderSample.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int idx = Array.FindIndex(parts, s => string.Equals(s, sampleName, StringComparison.OrdinalIgnoreCase));
            if (idx < 0)
            {
                return string.Empty;
            }

            // We expect parts like: Assets, Samples, VR Builder, <version>, <sampleDisplayName>, ...
            // So the root ends at <sampleDisplayName>.
            string root = string.Join("/", parts.Take(idx + 1));
            if (!root.StartsWith(samplesRootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return root;
        }

        /// <summary>
        /// Copies a file into the destination, it deletes the existing destination (including its .meta) 
        /// and then copies the source (GUID will change).
        /// 
        /// </summary>
        /// <param name="sourcePath">Project-relative source file path.</param>
        /// <param name="destinationPath">Project-relative destination file path.</param>
        /// <param name="overwrite">If true and destination exists, delete then copy.</param>
        /// <param name="error">Populated on failure with details.</param>
        /// <returns> True on success; otherwise false and <paramref name="error"/> contains details.</returns>
        /// <remarks> Paths must be project-relative (start with "Assets/").</remarks>
        public static bool TryCopyFile(string sourcePath, string destinationPath, out string error)
        {
            error = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
                {
                    error = "Source or destination path is null or empty.";
                    return false;
                }

                if (!File.Exists(sourcePath))
                {
                    error = $"Source does not exist: {sourcePath}";
                    return false;
                }

                string normalizedDest = destinationPath.Replace('\\', '/');
                string destDir = Path.GetDirectoryName(normalizedDest);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (File.Exists(normalizedDest))
                {
                    FileUtil.DeleteFileOrDirectory(normalizedDest);

                }

                FileUtil.CopyFileOrDirectory(sourcePath, normalizedDest);
                AssetDatabase.ImportAsset(normalizedDest);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }

        public static void OpenSampleScene(string demoSceneName, string samplesRootPrefix)
        {
            string scenePath = ResolveScenePathByName(demoSceneName, samplesRootPrefix);

            if (string.IsNullOrEmpty(scenePath))
            {
                UnityEngine.Debug.LogError($"[VR Builder - Sample Import] Could not find scene '{demoSceneName}' at '{samplesRootPrefix}'.");
                return;
            }

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            FixTeleportLayersForCurrentScene();
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            CheckBuiltInXRInteractionComponent();
        }

        /// <summary>
        /// Resolves the full asset path of a scene by its file name (without extension).
        /// </summary>
        /// <param name="sceneName"> The name of the scene to look for (e.g. "DemoScene" or "DemoScene.unity"). </param>
        /// <param name="pathRestriction"> Optional folder path to restrict the search (e.g. "Assets/Samples/VR Builder"). </param>
        /// <returns>
        /// The resolved Unity asset path of the scene (e.g. "Assets/Samples/VR Builder/0.0.0/Demo/DemoScene.unity"), or <see cref="string.Empty"/> if no matching scene was found.
        /// </returns>
        private static string ResolveScenePathByName(string sceneName, string pathRestriction = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                UnityEngine.Debug.LogError("[VR Builder - Sample Import] Scene name is null or empty.");
                return string.Empty;
            }

            string[] searchInFolders = string.IsNullOrEmpty(pathRestriction) ? null : new[] { pathRestriction };
            string[] guids = AssetDatabase.FindAssets($"{sceneName} t:Scene", searchInFolders);
            if (guids.Length == 0)
            {
                return string.Empty;
            }

            if (guids.Length > 1)
            {
                UnityEngine.Debug.LogWarning($"[VR Builder - Sample Import] Multiple scenes named '{sceneName}' found under '{pathRestriction ?? "Project"}'. Using the first result.");
            }

            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }


        public static void FixTeleportLayersForCurrentScene()
        {
#if VR_BUILDER && VR_BUILDER_XR_INTERACTION
            foreach (GameObject configuratorGameObject in GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).
                Where(go => go.GetComponent<VRBuilder.Core.Setup.ILayerConfigurator>() != null))
            {
                VRBuilder.Core.Setup.ILayerConfigurator configurator = configuratorGameObject.GetComponent<VRBuilder.Core.Setup.ILayerConfigurator>();
                if (configurator.LayerSet == VRBuilder.Core.Setup.LayerSet.Teleportation)
                {
                    configurator.ConfigureLayers("Teleport", "Teleport");
                    EditorUtility.SetDirty(configuratorGameObject);
                }
            }

            EditorSceneManager.SaveOpenScenes();
#endif
        }

        public static bool CheckBuiltInXRInteractionComponent()
        {
#if !VR_BUILDER_XR_INTERACTION
            if (EditorUtility.DisplayDialog("XR Interaction Component Required", "This demo scene requires VR Builder's built-in XR Interaction Component to be enabled. It looks like it is currently disabled. You can enable it in Project Settings > VR Builder > Settings.", "Ok")) 
            {
                return false;
            }
#endif
            return true;
        }
    }
}