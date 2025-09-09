#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Demo.Editor
{
    /// <summary>
    /// Reusable helpers for sample import postprocessing.
    /// </summary>
    public static class SampleImportPostprocessingUtility
    {
        private const string SourceStreamingAssetsFolderName = "StreamingAssets/Processes";
        private const string ProjectStreamingAssetsRoot = "Assets/StreamingAssets/Processes";
        private const string MarkerFileName = "ProcessesCopiedMarker.txt";

        /// <summary>
        /// Executes the copy/compare/marker workflow for a single sample.
        /// </summary>
        /// <param name="sampleRoot">Absolute asset path to the sample root folder.</param>
        public static void CopyProcessFile(string sampleRoot, string sampleName = "Hands Interaction Demo", string processFileName = "Hands Interaction Demo.json")
        {
            try
            {
                if (!Directory.Exists(sampleRoot))
                {
                    Debug.LogWarning($"[Sample Import - {sampleName}] Sample root not found: '{sampleRoot}'. Skipping.");
                    return;
                }

                string markerPath = Path.Combine(sampleRoot, MarkerFileName);
                if (File.Exists(markerPath))
                {
                    Debug.Log($"[Sample Import - {sampleName}] Marker found at '{markerPath}'. Skipping copy.");
                    return;
                }

                // Build source path inside the sample's /Assets/Samples/VR Builder/<version> + /StreamingAssets/Processes + /<DemoName> + /<File.json>
                string sourceJsonPath = Path.Combine(
                    Path.Combine(sampleRoot, SourceStreamingAssetsFolderName),
                    Path.Combine(sampleName, processFileName));

                if (!File.Exists(sourceJsonPath))
                {
                    Debug.LogError($"[Sample Import - {sampleName}] Source JSON not found at '{sourceJsonPath}'. Nothing to copy.");
                    return;
                }

                // Prepare destination directory: Assets/StreamingAssets/Processes/<DemoName>/
                string targetDir = Path.Combine(ProjectStreamingAssetsRoot, sampleName);
                string destinationJsonPath = Path.Combine(targetDir, processFileName);

                bool copied = TryCopyFile(sourceJsonPath, destinationJsonPath, out string copyError);
                if (copied)
                {
                    Debug.Log($"[Sample Import - {sampleName}] Copied process JSON to '{destinationJsonPath}'.");
                    TryCreateMarker(markerPath, $"Copied process file on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }
                else
                {
                    Debug.LogError($"[Sample Import - {sampleName}] Failed to copy new file. {copyError}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Sample Import - {sampleName}] Error processing sample root '{sampleRoot}': {ex}");
            }
        }

        /// <summary>
        /// From a list of changed asset paths, collects unique sample roots that match:
        /// Assets/Samples/VR Builder/<version>/<sampleDisplayName>/...
        /// </summary>
        /// <param name="changedPaths">Changed asset paths (imported or moved).</param>
        /// <param name="samplesRootPrefix">Prefix like 'Assets/Samples/VR Builder'.</param>
        /// <param name="sampleDisplayName">Sample folder name, e.g., 'Hands Interaction Demo'.</param>
        /// <param name="output">Destination list to which unique roots will be added.</param>
        public static void CollectSampleRootsFromChanges(
            string[] changedPaths,
            string samplesRootPrefix,
            string sampleDisplayName,
            List<string> output)
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

                string? root = TryResolveSampleRootFromPath(normalized, samplesRootPrefix, sampleDisplayName);
                if (!string.IsNullOrEmpty(root) && !output.Contains(root, StringComparer.OrdinalIgnoreCase))
                {
                    output.Add(root);
                    Debug.Log($"[VR Builder Sample Import] Candidate sample root found: '{root}' (from '{normalized}').");
                }
            }
        }

        /// <summary>
        /// Given a path under Assets/Samples/VR Builder/&lt;version&gt;/&lt;sampleDisplayName&gt;/...,
        /// returns the root path 'Assets/Samples/VR Builder/&lt;version&gt;/&lt;sampleDisplayName&gt;'.
        /// </summary>
        /// <param name="anyPathUnderSample">Any child path within the sample.</param>
        /// <param name="samplesRootPrefix">Prefix like 'Assets/Samples/VR Builder'.</param>
        /// <param name="sampleDisplayName">Sample folder name to anchor.</param>
        /// <returns>The resolved sample root path or empty string if not matched.</returns>
        public static string TryResolveSampleRootFromPath(string anyPathUnderSample, string samplesRootPrefix, string sampleDisplayName)
        {
            // Split into segments and locate the index of the sampleDisplayName.
            string[] parts = anyPathUnderSample.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int idx = Array.FindIndex(parts, s => string.Equals(s, sampleDisplayName, StringComparison.OrdinalIgnoreCase));
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
                string? destDir = Path.GetDirectoryName(normalizedDest);
                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (File.Exists(normalizedDest))
                {
                    FileUtil.DeleteFileOrDirectory(normalizedDest);

                    // Ensure lingering meta is removed too (Unity sometimes leaves it behind).
                    string metaPath = normalizedDest + ".meta";
                    if (File.Exists(metaPath))
                    {
                        FileUtil.DeleteFileOrDirectory(metaPath);
                    }
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
        /// Creates or overwrites a marker file that indicates the copy step was completed.
        /// </summary>
        public static void TryCreateMarker(string markerPath, string content)
        {
            try
            {
                File.WriteAllText(markerPath, content);
                AssetDatabase.ImportAsset(markerPath);
                Debug.Log($"[VR Builder Sample Import] Wrote marker file: '{markerPath}'.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VR Builder Sample Import] Failed to write marker file '{markerPath}': {ex}");
            }
        }
    }
}
#endif
