using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace VRBuilder.Demo.Editor
{
    public static class DemoSceneLoader
    {
        private const string packageName = "co.mindport.vrbuilder.core";

        [MenuItem("Tools/VR Builder/Example Scenes/Core Features", false, 63)]
        public static void LoadCoreFeaturesDemo()
        {
            LoadDemoScene(
                "Demo - Core Features",
                "VR Builder - Core Features Demo"
            );
        }

        [MenuItem("Tools/VR Builder/Example Scenes/Hands Interaction Demo", false, 64)]
        public static void LoadHandsInteractionDemo()
        {
            LoadDemoScene(
                "Demo - Hands Interaction",
                "VR Builder - Hands Interaction Demo",
                true
            );
        }

        private static void LoadDemoScene(string sampleFolderName, string sceneName, bool showDialog = false)
        {
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

            if (!sample.isImported)
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

            EditorApplication.delayCall += () =>
            {
                EditorUtility.ClearProgressBar();

                string newScenePath = FindScene(sceneName);

                if (!string.IsNullOrEmpty(newScenePath))
                {
                    OpenScene(newScenePath);
                }
                else
                {
                    Debug.LogError($"Scene '{sceneName}' not found after importing sample.");
                }
            };
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
    }
}