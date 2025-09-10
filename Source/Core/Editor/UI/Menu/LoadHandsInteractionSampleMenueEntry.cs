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
        private const string handsInteractionSample = "Demo - Hands Interaction";
        private const string nonePackagePath = "Assets/MindPort/VR Builder/Core/Samples~/Demo - Hands Interaction";
        private const string demoTargetDirectory = "Assets/Samples/VR Builder/0.0.0/Demo - Hands Interaction";
        private const string demoSceneName = "VR Builder - Hands Interaction Demo.unity";

        [MenuItem("Tools/VR Builder/Demo Scenes/Hands Interaction Demo", false, 64)]
        public static void ImportHandsInteractionDemo()
        {
            bool isPackage = !Directory.Exists(nonePackagePath);

            if (isPackage)
            {
                SampleImporter.ImportSampleFromPackage(packageName, handsInteractionSample);
            }
            else
            {
                try
                {
                    string targetParent = Path.GetDirectoryName(demoTargetDirectory);
                    if (Directory.Exists(targetParent))
                    {
                        bool confirmed = SampleImporter.ShowAlreadyImportedDialog(handsInteractionSample);
                        if (confirmed)
                        {
                            FileUtil.DeleteFileOrDirectory(targetParent);
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(targetParent) && !Directory.Exists(targetParent))
                    {
                        Directory.CreateDirectory(targetParent);
                    }
                    FileUtil.CopyFileOrDirectory(nonePackagePath, demoTargetDirectory);
                    AssetDatabase.Refresh();
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"Failed to copy demo from '{nonePackagePath}' to '{demoTargetDirectory}': {e.Message}");
                }
            }
        }
    }
}
