using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager.UI;
using System.IO;

namespace VRBuilder.Demo.Editor
{
    public static class DemoSceneLoader
    {
        private const string packageName = "co.mindport.vrbuilder.core";

        [MenuItem("Tools/VR Builder/Example Scenes/Core Features", false, 63)]
        public static void LoadBasicsDemo()
        {
            LoadDemoScene(
                packageName,
                "VR Builder - Core Features Demo"
            );
        }

        [MenuItem("Tools/VR Builder/Example Scenes/Hands Interaction Demo", false, 64)]
        public static void LoadHandInteractionDemo()
        {
            LoadDemoScene(
                packageName,
                "VR Builder - Hands Interaction Demo"
            );
        }
        private static void LoadDemoScene(string packageName, string sceneName)
        {
            // Try to find the sample scene in Assets/Samples
            string[] guids = AssetDatabase.FindAssets($"{sceneName} t:scene");
            string scenePath = guids.Select(AssetDatabase.GUIDToAssetPath)
                                    .FirstOrDefault(path => path.Contains("Samples"));

            if (!string.IsNullOrEmpty(scenePath))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(scenePath);
                return;
            }

            // If not found, open Package Manager and show instructions
            EditorUtility.DisplayDialog(
                "Import Demo Scene",
                $"The scene '{sceneName}' is included as a Sample in its package.\n\n" +
                "The Package Manager will open now.\n" +
                "Go to the 'Samples' tab and click 'Import' to get the demo scene.",
                "Open Package Manager"
            );

            Window.Open(packageName);
        }
    }
}