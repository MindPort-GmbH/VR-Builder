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
                
                //TODO enable TTS building for active scenes only
                //foreach (var sceneItem in EditorBuildSettings.scenes)
                //{
                //    Scene scene = SceneManager.GetSceneByPath(sceneItem.path);
                //    var runtimeConfigurator = scene.GetRootGameObjects()
                //        .Select(go => go.GetComponentInChildren<RuntimeConfigurator>(true))
                //        .FirstOrDefault(c => c != null);
                //    if (runtimeConfigurator != null)
                //    {
                //        Task task = TextToSpeechEditorUtils.GenerateTextToSpeechForProcess(runtimeConfigurator.GetSelectedProcess());
                //        task.Wait();
                //    }
                //    else
                //    {
                //        UnityEngine.Debug.LogWarning($"No TTS files for the scene {scene.name} could be generated. Error RuntimeConfigurator component was not found in the scene.");
                //    }
                //}
            }
            else
            {
                UnityEngine.Debug.LogWarning("Text to speech files generation is disabled.");
            }
        }
    }
}