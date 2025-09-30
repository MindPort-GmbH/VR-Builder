using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using System;

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
        /// Imports a sample from the Assets folder into the specified demo target directory.
        /// </summary>
        /// <param name="sampleName">The sample identifier used to display a confirmation dialog if a previous installation exists.</param>
        /// <param name="nonePackagePath">The file or directory path of the sample to import.</param>
        /// <param name="demoTargetDirectory">The destination directory where the sample should be copied.</param>
        /// <returns>
        /// Returns <c>true</c> if the import completed successfully; returns <c>false</c> if the user canceled the overwrite when the target already existed.
        /// </returns>
        public static bool ImportSampleFromAssets(string sampleName, string nonePackagePath, string demoTargetDirectory)
        {
            try
            {
                string targetParent = Path.GetDirectoryName(demoTargetDirectory);
                bool sampleExists = false;

                if (!string.IsNullOrEmpty(demoTargetDirectory))
                {
                    sampleExists = Directory.Exists(demoTargetDirectory) ||
                                   File.Exists(demoTargetDirectory) ||
                                   AssetDatabase.IsValidFolder(demoTargetDirectory);
                }

                if (sampleExists)
                {
                    bool confirmed = ShowAlreadyImportedDialog(sampleName, demoTargetDirectory);
                    if (confirmed)
                    {
                        FileUtil.DeleteFileOrDirectory(demoTargetDirectory);
                    }
                    else
                    {
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(targetParent) && !Directory.Exists(targetParent))
                {
                    Directory.CreateDirectory(targetParent);
                }

                FileUtil.CopyFileOrDirectory(nonePackagePath, demoTargetDirectory);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to copy demo from '{nonePackagePath}' to '{demoTargetDirectory}': {e.Message}");
            }

            return true;
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
                bool confirmed = ShowAlreadyImportedDialog(sample.displayName, sample.importPath);
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
        /// <param name="sampleName">The name of the sample.</param>
        /// <param name="sampleDestinationPath">The path to the install path.</param>
        /// <returns>True if the user clicked "Yes", false if "No".</returns>
        public static bool ShowAlreadyImportedDialog(string sampleName, string sampleDestinationPath = null)
        {
            string title = "Importing package sample";

            string message;
            if (string.IsNullOrEmpty(sampleDestinationPath))
            {
                message = $"The sample '{sampleName}' is already imported.\n\n" +
                        "Importing again will remove the previous versions and override all changes you have made to it." +
                        "Are you sure you want to continue?";
            }
            else
            {
                message = $"The sample '{sampleName}' is already imported.\n\n" +
                        "Importing again will remove the previous versions and override all changes you have made to it.\n\n" +
                        $"The new package will be installed to {sampleDestinationPath}\n\n" +
                        "Are you sure you want to continue?";
            }

            return EditorUtility.DisplayDialog(
                title,
                message,
                "Yes",
                "No"
            );
        }
    }
}

