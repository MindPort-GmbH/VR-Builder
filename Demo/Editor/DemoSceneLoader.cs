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
        private const string demoScenePath = "Assets/MindPort/VR Builder/Core/Demo/Runtime/Scenes/VR Builder Demo - Core.unity";
        private const string demoProcessFilePath = "Assets/StreamingAssets/Processes/Demo - Core/Demo - Core.json";

        [MenuItem("Tools/VR Builder/Demo Scenes/Core", false, 64)]
        public static void LoadDemoScene()
        {
#if !VR_BUILDER_ENABLE_XR_INTERACTION
            EditorUtility.DisplayDialog("XR Interaction Component Required", "This demo scene requires VR Builder's default XR Interaction Component to be enabled. It looks like it is currently disabled. You can enable it in Project Settings > VR Builder > Settings.", "Ok");
            return;
#endif
            if (File.Exists(demoProcessFilePath) == false)
            {
                if(EditorUtility.DisplayDialog("Demo Scene Setup", "Before opening the demo scene, the sample process needs to be copied in Assets/StreamingAssets. Press Ok to proceed.", "Ok"))
                {
                    Directory.CreateDirectory("Assets/StreamingAssets/Processes/Demo - Core");
                    FileUtil.CopyFileOrDirectory("Assets/MindPort/VR Builder/Core/Demo/StreamingAssets/Processes/Demo - Core/Demo - Core.json", demoProcessFilePath);
                }
            }

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(demoScenePath);
        }
    }
}