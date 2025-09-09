using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Demo.Editor;

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

        // Track which sample roots we processed within the same import batch (avoid duplicate dialogs).
        private static readonly HashSet<string> s_processedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                // In our case we know that there is only one candidate as we remove all other samples from diferent versions.
                SampleImportPostprocessingUtility.CopyProcessFile(candidateRoots[0], sampleName, processFileName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Sample Import - {sampleName}] Unexpected error in postprocess: {ex}");
            }
            finally
            {
                // Clear the set at the end of the update loop to avoid memory growth between batches.
                // Unity calls this method per import batch, so we can safely reset here.
                s_processedRoots.Clear();
            }
        }
    }
}