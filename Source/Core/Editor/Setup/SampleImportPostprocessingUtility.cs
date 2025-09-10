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
        private const string samplesRootPrefix = "Assets/Samples/VR Builder";
        private const string SourceStreamingAssetsFolderName = "StreamingAssets/Processes";
        private const string ProjectStreamingAssetsRoot = "Assets/StreamingAssets/Processes";
        private const string MarkerFileName = "ProcessesCopiedFlag.md";

        /// <summary>
        /// Executes the copy/compare/marker workflow for a single sample.
        /// </summary>
        /// <param name="sampleRoot">Absolute asset path to the sample root folder.</param>
        public static bool CopyProcessFile(string sampleRoot, string sampleName = "Hands Interaction Demo", string processFileName = "Hands Interaction Demo.json")
        {
            try
            {
                if (!Directory.Exists(sampleRoot))
                {
                    UnityEngine.Debug.LogError($"[Sample Import - {sampleName}] Sample root not found: '{sampleRoot}'. Skipping.");
                    return false;
                }

                string markerPath = Path.Combine(sampleRoot, MarkerFileName);
                if (File.Exists(markerPath))
                {
                    // UnityEngine.Debug.Log($"[Sample Import - {sampleName}] Marker found at '{markerPath}'. Skipping copy.");
                    return false;
                }

                // Build source path inside the sample's /Assets/Samples/VR Builder/<version> + /StreamingAssets/Processes + /<DemoName> + /<File.json>
                string sourceJsonPath = Path.Combine(
                    Path.Combine(sampleRoot, SourceStreamingAssetsFolderName),
                    Path.Combine(sampleName, processFileName));

                if (!File.Exists(sourceJsonPath))
                {
                    UnityEngine.Debug.LogError($"[Sample Import - {sampleName}] Source JSON not found at '{sourceJsonPath}'. Nothing to copy.");
                    return false;
                }

                // Prepare destination directory: Assets/StreamingAssets/Processes/<DemoName>/
                string targetDir = Path.Combine(ProjectStreamingAssetsRoot, sampleName);
                string destinationJsonPath = Path.Combine(targetDir, processFileName);

                bool copied = TryCopyFile(sourceJsonPath, destinationJsonPath, out string copyError);
                if (copied)
                {
                    // UnityEngine.Debug.Log($"[Sample Import - {sampleName}] Copied process JSON to '{destinationJsonPath}'.");
                    TryCreateProcessCopiedFlag(markerPath, $"This file indicates that the process file for the sample '{sampleName}' was copied to StreamingAssets on {DateTime.Now:yyyy-MM-dd HH:mm:ss}. You can delete this file if you want to force the copy to run again.");
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[Sample Import - {sampleName}] Failed to copy new file. {copyError}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Sample Import - {sampleName}] Error processing sample root '{sampleRoot}': {ex}");
                return false;
            }
        }

        /// <summary>
        /// From a list of changed asset paths, collects unique sample roots that match:
        /// Assets/Samples/VR Builder/<version>/<sampleDisplayName>/...
        /// </summary>
        /// <param name="changedPaths">Changed asset paths (imported or moved).</param>
        /// <param name="samplesRootPrefix">Prefix like 'Assets/Samples/VR Builder'.</param>
        /// <param name="sampleName">Sample folder name, e.g., 'Hands Interaction Demo'.</param>
        /// <param name="output">Destination list to which unique roots will be added.</param>
        public static void CollectSampleRootsFromChanges(string[] changedPaths, string sampleName, List<string> output)
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
                    // UnityEngine.Debug.Log($"[VR Builder Sample Import] Candidate sample root found: '{root}' (from '{normalized}').");
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

        /// <summary>
        /// Creates or overwrites a process copied flag file that indicates the copy step was completed.
        /// </summary>
        public static void TryCreateProcessCopiedFlag(string markerPath, string content)
        {
            try
            {
                File.WriteAllText(markerPath, content);
                AssetDatabase.ImportAsset(markerPath);
                // UnityEngine.Debug.Log($"[VR Builder Sample Import] Wrote processes copied flag file: '{markerPath}'.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[VR Builder Sample Import] Failed to write processes copied flag file '{markerPath}': {ex}");
            }
        }

        public static void OpenSampleScene(string demoSceneName)
        {
            string scenePath = ResolveScenePathByName(demoSceneName, samplesRootPrefix);

            if (string.IsNullOrEmpty(scenePath))
            {
                UnityEngine.Debug.LogError($"[SampleImportPostprocessing] Could not find scene '{demoSceneName}' under '{samplesRootPrefix}'.");
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
                UnityEngine.Debug.LogError("[SampleImportPostprocessing] Scene name is null or empty.");
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
                UnityEngine.Debug.LogWarning($"[SampleImportPostprocessing] Multiple scenes named '{sceneName}' found under '{pathRestriction ?? "Project"}'. Using the first result.");
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