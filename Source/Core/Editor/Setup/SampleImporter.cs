using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using System;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace VRBuilder.Core.Editor.Setup
{
    public static class SampleImporter
    {
        /// <summary>
        /// Removes all previous versions of the selected sample and imports the sample of the current version.
        /// </summary>
        /// <param name="packageName">The name of the package containing the sample.</param>
        /// <param name="sampleName">The display name of the sample to import.</param>
        /// <param name="packageVersion">The version of the package to use. If null or empty, the installed version is used.</param>
        /// <returns>True if the sample was found and import was triggered; otherwise, false.</returns>
        internal static bool ImportSampleFromPackage(string packageName, string sampleName, string packageVersion = null)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                UnityEngine.Debug.LogError("ImportSample failed: packageName is null or empty.");
                return false;
            }

            if (string.IsNullOrEmpty(sampleName))
            {
                UnityEngine.Debug.LogError("ImportSample failed: sampleDisplayName is null or empty.");
                return false;
            }

            // Unity’s Sample.FindByPackage will fall back to the installed version if version is null/empty.
            Sample[] samples = Sample.FindByPackage(packageName, packageVersion)?.ToArray();
            if (samples == null || samples.Length == 0)
            {
                UnityEngine.Debug.LogError($"No samples found for package '{packageName}' (version: '{packageVersion ?? "installed"}'). Is the package installed and does it contain samples?");
                return false;
            }

            foreach (Sample sample in samples)
            {
                if (sample.displayName == sampleName)
                {
                    return ImportSample(sample);
                }
            }

            UnityEngine.Debug.LogError($"Sample '{sampleName}' not found in package '{packageName}' (version: '{packageVersion ?? "installed"}').");
            return false;
        }

        /// <summary>
        /// Imports a sample from the package. If the sample has been previously imported,
        /// it prompts the user for confirmation to override the existing import.
        /// </summary>
        /// <param name="sampleName">The name of the sample to import.</param>
        /// <param name="sample">The sample instance to be imported.</param>
        /// <returns>
        /// True if the sample was successfully imported or re-imported after user confirmation;
        /// otherwise, false if the import did not occur.
        /// </returns>
        private static bool ImportSample(Sample sample)
        {
            if (sample.isImported)
            {
                bool confirmed = ShowAlreadyImportedDialog(sample.displayName);
                if (confirmed)
                {
                    sample.Import(Sample.ImportOptions.OverridePreviousImports);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                sample.Import();
                return true;
            }
        }

        /// <summary>
        /// Shows a dialog asking if the user wants to re-import a sample.
        /// </summary>
        /// <param name="samplePath">The path to the already imported sample.</param>
        /// <returns>True if the user clicked "Yes", false if "No".</returns>
        public static bool ShowAlreadyImportedDialog(string sampleName)
        {
            string title = "Importing package sample";
            string message =
                $"The sample '{sampleName}' is already imported.\n\n" +
                "Importing again will remove the previous versions and override all changes you have made to it. " +
                "Are you sure you want to continue?";

            return EditorUtility.DisplayDialog(
                title,
                message,
                "Yes",
                "No"
            );
        }


        internal static void PostSampleImportTasks(string handsInteractionSample, string importPath, string demoProcessTargetDirectory, string demoScenePath)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(demoScenePath);

            FixTeleportLayersForCurrentScene();
            AssetDatabase.Refresh();

            CheckBuiltInXRInteractionComponent();
        }

        internal static void FixTeleportLayersForCurrentScene()
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

        internal static bool CheckBuiltInXRInteractionComponent()
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
