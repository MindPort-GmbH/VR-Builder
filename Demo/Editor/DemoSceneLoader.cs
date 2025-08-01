using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VRBuilder.Demo.Editor
{
    /// <summary>
    /// Menu item for loading the demo scene after checking the process file is in the StreamingAssets folder.
    /// </summary>
    public static class DemoSceneLoader
    {
        private const string demoSceneAssetsPath = "Assets/MindPort/VR Builder/Core/Demo/Scenes/VR Builder Demo - Core Features.unity";
        private const string demoScenePackagesPath = "Packages/co.mindport.vrbuilder.core/Demo/Scenes/VR Builder Demo - Core Features.unity";

        private const string demoProcessAssetsPath = "Assets/MindPort/VR Builder/Core/Demo/StreamingAssets/Processes/Demo - Core Features/Demo - Core Features.json";
        private const string demoProcessPackagesPath = "Packages/co.mindport.vrbuilder.core/Demo/StreamingAssets/Processes/Demo - Core Features/Demo - Core Features.json";

        private const string demoProcessTargetPath = "Assets/StreamingAssets/Processes/Demo - Core Features/Demo - Core Features.json";
        private const string demoProcessTargetDirectory = "Assets/StreamingAssets/Processes/Demo - Core Features";

        [MenuItem("Tools/VR Builder/Demo Scenes/Core", false, 63)]
        public static void LoadDemoScene()
        {
#if !VR_BUILDER_XR_INTERACTION
            if (EditorUtility.DisplayDialog("XR Interaction Component Required", "This demo scene requires VR Builder's built-in XR Interaction Component to be enabled. It looks like it is currently disabled. You can enable it in Project Settings > VR Builder > Settings.", "Ok")) 
            {
                return;
            }
#endif
            bool isUpmPackage = File.Exists(demoScenePackagesPath);

            string demoScenePath = isUpmPackage ? demoScenePackagesPath : demoSceneAssetsPath;
            string demoProcessPath = isUpmPackage ? demoProcessPackagesPath : demoProcessAssetsPath;

            if (File.Exists(demoProcessTargetPath) == false)
            {
                Directory.CreateDirectory(demoProcessTargetDirectory);
                FileUtil.CopyFileOrDirectory(demoProcessPath, demoProcessTargetPath);
            }

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(demoScenePath);

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

            AssetDatabase.Refresh();
        }
    }
}