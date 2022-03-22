using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace VRBuilder.Editor.DemoScene
{
    /// <summary>
    /// Menu item for loading the demo scene after checking the process file is in the StreamingAssets folder.
    /// </summary>
    public static class DemoSceneLoader
    {
        private const string demoScenePath = "Assets/MindPort/VR Builder/Add-ons/Downloader/Source/DemoScene/Runtime/Scenes/VR Builder Demo - Core.unity";
        private const string demoProcessFilePath = "Assets/StreamingAssets/Processes/Demo - Core/Demo - Core.json";

        [MenuItem("Tools/VR Builder/Demo Scenes/Core", false, 64)]
        public static void LoadDemoScene()
        {
            if (File.Exists(demoProcessFilePath) == false)
            {
                if(EditorUtility.DisplayDialog("Demo Scene Setup", "Before opening the demo scene, the sample process needs to be copied in Assets/StreamingAssets. Press Ok to proceed.", "Ok"))
                {
                    Directory.CreateDirectory("Assets/StreamingAssets/Processes/Demo - Core");
                    FileUtil.CopyFileOrDirectory("Assets/MindPort/VR Builder/Add-ons/Downloader/Source/StreamingAssets/Processes/Demo - Core/Demo - Core.json", demoProcessFilePath);
                }
            }

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(demoScenePath);
        }
    }
}