using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VRBuilder.Core;
using VRBuilder.TextToSpeech.Audio;

namespace VRBuilder.Editor.TextToSpeech
{
    public class TextToSpeechBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            IEnumerable<string> processNames = ProcessAssetUtils.GetAllProcesses();

            foreach (string processName in processNames)
            { 
                IProcess process = ProcessAssetManager.Load(processName);

                if(process != null)
                {
                    IEnumerable<TextToSpeechAudio> tts = EditorReflectionUtils.GetPropertiesFromProcess<TextToSpeechAudio>(process);

                    if (tts.Count() > 0)
                    {
                        Debug.Log($"Building audio files for process '{process.Data.Name}...'");
                        TextToSpeechEditorUtils.CacheTextToSpeechClips(tts);
                    }
                }
            }
        }
    }
}