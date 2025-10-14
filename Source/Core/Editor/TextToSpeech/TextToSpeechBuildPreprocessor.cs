using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using VRBuilder.Core.Editor.TextToSpeech.Utils;
using VRBuilder.Core.TextToSpeech;

namespace VRBuilder.Core.Editor.TextToSpeech
{
    /// <summary>
    /// Generates TTS files for all processes before a build.
    /// </summary>
    public class TextToSpeechBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        /// <summary>
        /// Generates TTS files for all processes before a build.
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (TextToSpeechSettings.Instance.GenerateAudioInBuildingProcess)
            {
                var task = TextToSpeechEditorUtils.GenerateTextToSpeechForBuildScenes();
                task.Wait();
            }
            else
            {
                UnityEngine.Debug.LogWarning("Text to speech files generation is disabled.");
            }
        }
    }
}