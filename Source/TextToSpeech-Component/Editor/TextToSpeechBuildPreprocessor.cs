using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.TextToSpeech.Audio;

namespace VRBuilder.Editor.TextToSpeech
{
    /// <summary>
    /// Generates TTS files for all processes before a build.
    /// </summary>
    public class TextToSpeechBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        /// <summary>
        /// Generates TTS files for all processes before a build.
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            IEnumerable<string> processNames = ProcessAssetUtils.GetAllProcesses();

            foreach (string processName in processNames)
            { 
                IProcess process = ProcessAssetManager.Load(processName);

                if(process != null)
                {
                    IEnumerable<ITextToSpeechContent> tts = EditorReflectionUtils.GetPropertiesFromProcess<ITextToSpeechContent>(process).Where(content => content.IsCached == false);

                    if (tts.Count() > 0)
                    {
                        Debug.Log($"Generating {tts.Count()} audio files for process '{process.Data.Name}...'");
                        TextToSpeechEditorUtils.CacheTextToSpeechClips(tts);
                    }
                }
            }
        }
    }
}