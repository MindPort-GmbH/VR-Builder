using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Editor.Setup;
using VRBuilder.XRInteraction.Editor.Validation;


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
        private const string sampleRootPrefix = "Assets/Samples/VR Builder";

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
                SampleImportPostprocessingUtility.CollectSampleRootsFromChanges(importedAssets, sampleName, candidateRoots, sampleRootPrefix);

                if (candidateRoots.Count == 0)
                {
                    return;
                }

                if (candidateRoots.Count > 1)
                {
                    Debug.LogWarning($"[VR Builder - {sampleName}] Multiple candidate sample roots found in the import batch. This is unexpected, but we will proceed with the first candidate. Candidates:\n{string.Join("\n", candidateRoots)}");
                }

                string sampleRootPath = candidateRoots[0];
                if (!SampleImportPostprocessingUtility.IsSampleImportedFlagSet(sampleRootPath))
                {
                    SampleImportPostprocessingUtility.InitiateImportPostprocessing(sampleRootPath, sampleName, processFileName, demoSceneName, GetFixValidationIssuesAction());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VR Builder - {sampleName}] Unexpected error in postprocess: {ex}");
            }
        }

        private static Action GetFixValidationIssuesAction()
        {
            Action handsSampleProjectValidationAction = HandsSampleProjectValidation.FixAllValidationIssues;
            Action openXRHandsProjectValidationAction = OpenXRHandsProjectValidation.FixAllValidationIssues;
            return () =>
            {
                handsSampleProjectValidationAction();
                openXRHandsProjectValidationAction();
            };
        }
    }
}