using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRBuilder.Core.Editor.Setup;

namespace VRBuilder.Demo.Editor
{
    public static class DemoSceneLoader
    {
        private const string packageName = "co.mindport.vrbuilder.core";
        private const string samplesRoot = "Assets/Samples/VR Builder";
        private const string packageProcessRoot = "Packages/co.mindport.vrbuilder.core/StreamingAssets~/Processes/";
        private const string packageProcessDestination = "Assets/StreamingAssets/Processes/";

        [MenuItem("Tools/VR Builder/Example Scenes/Core Features", false, 63)]
        public static void LoadCoreFeaturesDemo()
        {
            LoadDemoScene(
                "Demo - Core Features",
                "VR Builder - Core Features Demo",
                "Demo - Core Features.json"
            );
        }

        [MenuItem("Tools/VR Builder/Example Scenes/Hands Interaction Demo", false, 64)]
        public static void LoadHandsInteractionDemo()
        {
            LoadDemoScene(
                "Demo - Hands Interaction",
                "VR Builder - Hands Interaction Demo",
                "Demo - Hands Interaction.json"
            );
        }

        private static void LoadDemoScene(string sampleFolderName, string sceneName,string processFileName, bool showDialog = false)
        {

            // Check Version
            string version = GetPackageVersion();
            // Remove old versions of samples to prevent confusion and potential issues with outdated content
            CleanupOldSampleVersions(version);

            string scenePath = FindScene(sceneName);

            if (!string.IsNullOrEmpty(scenePath))
            {
                OpenScene(scenePath);
                return;
            }

            var sample = Sample.FindByPackage(packageName, null)
                .FirstOrDefault(s => s.resolvedPath.Replace("\\", "/").Contains(sampleFolderName));

            if (string.IsNullOrEmpty(sample.displayName))
            {
                Debug.LogError($"Sample '{sampleFolderName}' not found in package '{packageName}'.");
                return;
            }

            if (!sample.isImported || !SampleVersionExists(version))
            {
                if (showDialog)
                {
                    // Inform user
                    bool startImport = EditorUtility.DisplayDialog(
                                         "Import Demo",
                                         $"The demo '{sample.displayName}' requires importing sample assets and dependencies.\n\n" +
                                         "This may take a few moments depending on your project.",
                                         "Import Demo",
                                         "Cancel"
);

                    if (!startImport)
                    {
                        return;
                    }

                    EditorUtility.DisplayProgressBar(
                        "VR Builder",
                        $"Importing sample '{sample.displayName}'...",
                        0.5f
                    );
                }
                sample.Import();
            }
        }

        private static string FindScene(string sceneName)
        {
            return AssetDatabase.FindAssets($"{sceneName} t:scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.Contains("Samples"));
        }

        private static void OpenScene(string path)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(path);
        }

        private static bool SampleVersionExists(string version)
        {
            string path = $"{samplesRoot}/{version}";
            return AssetDatabase.IsValidFolder(path);
        }
        private static void CleanupOldSampleVersions(string currentVersion)
        {
            if (!AssetDatabase.IsValidFolder(samplesRoot))
                return;

            string[] folders = AssetDatabase.GetSubFolders(samplesRoot);

            foreach (string folder in folders)
            {
                string folderName = System.IO.Path.GetFileName(folder);

                if (folderName != currentVersion)
                {
                    Debug.Log($"Removing old VR Builder samples: {folder}");
                    AssetDatabase.DeleteAsset(folder);
                }
            }

            AssetDatabase.Refresh();
        }

        private static string GetPackageVersion()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{packageName}");
            return info?.version;
        }
    }
}