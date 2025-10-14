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

        [MenuItem("Tools/VR Builder/Demo Scenes/Hands Interaction Demo", false, 68)]
        public static void ImportHandsInteractionDemo()
        {
            bool isPackage = !Directory.Exists(assetsSampleSourcePath);

            // When updating the dialog text, also update it in sample text of the package.json
            int result = EditorUtility.DisplayDialogComplex(
                "Demo - Hands Interaction",
                "This demo includes a sample scene for hand tracking with VR Builder and the XR Interaction Toolkit.\n\n" +
                "Importing this demo will install the following dependencies:\n" +
                " • Package: OpenXR (com.unity.xr.openxr).\n" +
                " • Package: XR Hands (com.unity.xr.hands).\n" +
                " • Package: Shader Graph (com.unity.shadergraph).\n" +
                " • Sample: XR Hands - HandVisualizer.\n" +
                " • Sample: XRI - Starter Assets.\n" +
                " • Sample: XRI - Hands Interaction Demo.\n\n" +
                "It may take a bit of time, but at the end the sample scene will be opened automatically.",
                "Continue",
                "Cancel",
                ""
            );

            if (result != 0)
            {
                return;
            }

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
