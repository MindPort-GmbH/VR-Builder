using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRBuilder.Core.Editor.Setup;

namespace VRBuilder.Core.Editor
{
    public static class LoadHandsInteractionSampleMenueEntry
    {
        private const string packageName = "co.mindport.vrbuilder.core";
        private const string handsInteractionSample = "VR Builder - Hands Interaction Demo";

        private const string demoSceneAssetsPath = "Assets/MindPort/VR Builder/Core/Samples~/VR Builder - Hands Interaction Demo/VR Builder - Hands Interaction Demo.unity";
        private const string demoProcessTargetPath = "VR Builder - Hands Interaction Demo/StreamingAssets/Processes/VR Builder - Hands Interaction Demo/VR Builder - Hands Interaction Demo.json";
        private const string demoProcessTargetDirectory = "Assets/StreamingAssets/Processes/VR Builder - Hands Interaction Demo/VR Builder - Hands Interaction Demo.json";

        [MenuItem("Tools/VR Builder/Demo Scenes/Hands Interaction Demo", false, 64)]
        public static void ImportHandsInteractionDemo()
        {
            bool isPackage = !File.Exists(demoSceneAssetsPath);

            if (isPackage)
            {
                string importPath;
                bool imported = SampleImporter.OverrideImportSample(packageName, handsInteractionSample, out importPath);

                if (imported)
                {
                    PostSampleImportTasks(importPath);
                }
            }
            else
            {

                // TODO remove all sub folders and containing files with name "VR Builder - Hands Interaction Demo" in Assets/Samples/. Use UnityEditor.Directory or UnityEditor.FileUtil where aplicable
                // TODO copy Assets/MindPort/VR Builder/Core/Samples~/VR Builder - Hands Interaction Demo to Assets/ to Assets/Samples/VR Builder/VR Builder - Hands Interaction Demo to Assets/. Use UnityEditor.Directory or UnityEditor.FileUtil where aplicable.

                string importPath = "Assets/Samples/VR Builder/VR Builder - Hands Interaction Demo to Assets/";
                PostSampleImportTasks(importPath);
            }
        }

        public static void PostSampleImportTasks(string importPath)
        {

            //SampleImporter.PostSampleImportTasks(handsInteractionSample, importPath, demoProcessTargetDirectory, demoScenePath);

            //SampleImporter.SetupProcessFile(HandsInteractionSample);
            //SetupProcessFile();

            // if (!BuiltInXRInteractionComponentActive())
            // {
            //     return;
            // }




            SampleImporter.CheckBuiltInXRInteractionComponent();
        }


        private static void SetupProcessFile()
        {
            if (!File.Exists(demoProcessTargetPath))
            {
                Directory.CreateDirectory(demoProcessTargetDirectory);
                //FileUtil.CopyFileOrDirectory(demoScenePath, demoProcessTargetPath);
            }
            else
            {
                // TODO Ask to overwrite
            }
        }

    }
}
