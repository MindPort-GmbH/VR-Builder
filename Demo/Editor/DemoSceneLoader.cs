using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VRBuilder.Editor.DemoScene
{
    /// <summary>
    /// Menu item for loading the demo scene after checking the process file is in the StreamingAssets folder.
    /// </summary>
    public static class DemoSceneLoader
    {
        private const string demoScenePath = "Assets/MindPort/VR Builder/Core/Demo/Runtime/Scenes/VR Builder Demo - Core Features.unity";
        private const string demoProcessFilePath = "Assets/StreamingAssets/Processes/Demo - Core Features/Demo - Core Features.json";

        [MenuItem("Tools/VR Builder/Demo Scenes/Core", false, 64)]
        public static void LoadDemoScene()
        {
#if !VR_BUILDER_XR_INTERACTION
            if (EditorUtility.DisplayDialog("XR Interaction Component Required", "This demo scene requires VR Builder's built-in XR Interaction Component to be enabled. It looks like it is currently disabled. You can enable it in Project Settings > VR Builder > Settings.", "Ok")) 
            {
                return;
            }
#endif

            if (File.Exists(demoProcessFilePath) == false)
            {
                Directory.CreateDirectory("Assets/StreamingAssets/Processes/Demo - Core Features");
                FileUtil.CopyFileOrDirectory("Assets/MindPort/VR Builder/Core/Demo/StreamingAssets/Processes/Demo - Core Features/Demo - Core Features.json", demoProcessFilePath);
            }

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(demoScenePath);

#if VR_BUILDER && VR_BUILDER_XR_INTERACTION
            foreach (GameObject configuratorGameObject in GameObject.FindObjectsOfType<GameObject>(true).
                Where(go => go.GetComponent<VRBuilder.Core.Setup.ILayerConfigurator>() != null))
            {
                VRBuilder.Core.Setup.ILayerConfigurator configurator = configuratorGameObject.GetComponent<VRBuilder.Core.Setup.ILayerConfigurator>();
                if (configurator.LayerSet == VRBuilder.Core.Setup.LayerSet.Teleportation)
                {
                    configurator.ConfigureLayers("XR Teleport", "XR Teleport");
                    EditorUtility.SetDirty(configuratorGameObject);
                }
            }

            EditorSceneManager.SaveOpenScenes();
#endif

            AssetDatabase.Refresh();
        }
    }
}