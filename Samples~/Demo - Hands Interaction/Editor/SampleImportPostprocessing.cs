using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.Setup;


namespace VRBuilder.Samples.HandsInteraction.Editor
{
    /// <summary>
    /// Copies the Hands Interaction process JSON from the sample's StreamingAssets
    /// to the project's main StreamingAssets immediately after import.
    /// </summary>
    public sealed class SampleImportPostprocessing : AssetPostprocessor
    {
        // ---------- Constants specific to THIS sample ----------
        private const string sampleName = "Demo - Hands Interaction";
        private const string processFileName = "Demo - Hands Interaction.json";
        private const string demoSceneName = "VR Builder - Hands Interaction Demo";

        /// <summary>
        /// Unity callback invoked after assets are imported, deleted, or moved.
        /// We probe the changed lists for our specific sample folder and act once per sample root.
        /// </summary>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            try
            {
                // Gather candidate sample roots from changed paths.
                List<string> candidateRoots = new List<string>();
                SampleImportPostprocessingUtility.CollectSampleRootsFromChanges(importedAssets, sampleName, candidateRoots);

                if (candidateRoots.Count == 0)
                {
                    return;
                }

                if (candidateRoots.Count > 1)
                {
                    Debug.LogWarning($"[Sample Import - {sampleName}] Multiple candidate sample roots found in the import batch. This is unexpected, but we will proceed with the first candidate. Candidates:\n{string.Join("\n", candidateRoots)}");
                }

                // In our case we know that there is only one candidate as we remove all other samples from different versions.
                InitiateImportPostprocessing(candidateRoots[0], sampleName, processFileName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Sample Import - {sampleName}] Unexpected error in postprocess: {ex}");
            }
        }

        private static void InitiateImportPostprocessing(string path, string sampleName, string processFileName)
        {

            bool success = SampleImportPostprocessingUtility.CopyProcessFile(path, sampleName, processFileName);
            if (success)
            {
                FixAllIssuesSafely();
                EditorApplication.delayCall += () => OpenOpenSampleSceneSafely(demoSceneName);
            }
        }

        public static void FixAllIssuesSafely()
        {
            if (AssetDatabase.IsAssetImportWorkerProcess() ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += FixAllIssuesSafely;
                Debug.Log("[Sample Import - Hands Interaction] Delaying project validation until the editor is in a stable state.");
                return;
            }
            Debug.Log("[Sample Import - Hands Interaction] Running project validation.");
            HandsSampleProjectValidation.FixValidationIssues();
        }

        public static void OpenOpenSampleSceneSafely(string demoSceneName)
        {
            if (AssetDatabase.IsAssetImportWorkerProcess() ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.Log("[Sample Import - Hands Interaction] Delaying opening the demo scene until the editor is in a stable state.");
                EditorApplication.delayCall += () => OpenOpenSampleSceneSafely(demoSceneName);
                return;
            }
            Debug.Log("[Sample Import - Hands Interaction] Opening the demo scene.");
            SampleImportPostprocessingUtility.OpenSampleScene(demoSceneName);
        }
    }
}