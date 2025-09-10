using System.IO;
using UnityEditor;
using VRBuilder.Core.Editor.Setup;

namespace VRBuilder.Core.Editor
{
    public static class LoadHandsInteractionSampleMenuEntry
    {
        private const string packageName = "co.mindport.vrbuilder.core";
        private const string sampleName = "Demo - Hands Interaction";
        private const string assetsSampleSourcePath = "Assets/MindPort/VR Builder/Core/Samples~/" + sampleName;
        private const string assetsSampleTargetDirectory = "Assets/Samples/VR Builder/0.0.0/" + sampleName;

        [MenuItem("Tools/VR Builder/Demo Scenes/Hands Interaction Demo", false, 64)]
        public static void ImportHandsInteractionDemo()
        {
            bool isPackage = !Directory.Exists(assetsSampleSourcePath);

            if (isPackage)
            {
                SampleImporter.ImportSampleFromPackage(packageName, sampleName);
            }
            else
            {
                SampleImporter.ImportSampleFromAssets(sampleName, assetsSampleSourcePath, assetsSampleTargetDirectory);
            }
        }
    }
}
