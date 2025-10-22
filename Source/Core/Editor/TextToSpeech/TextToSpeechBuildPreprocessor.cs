using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.TextToSpeech.Utils;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.TextToSpeech
{
    /// <summary>
    /// Generates TTS files for all processes before a build.
    /// </summary>
    public class TextToSpeechBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (TextToSpeechSettings.Instance.GenerateAudioInBuildingProcess)
            {
                Task task = TextToSpeechEditorUtils.GenerateTextToSpeechForAllProcesses();
                task.Wait();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Automated text to speech files generation is disabled. Please ensure files are up to date before building!");
            }
        }
    }
}