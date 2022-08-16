using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VRBuilder.Core.Internationalization;
using VRBuilder.TextToSpeech;
using VRBuilder.Unity;

namespace VRBuilder.TextToSpeech
{
    public class Mimic3TextToSpeechProvider : ITextToSpeechProvider
    {
        private const string URL = "http://lin-01.int.lefx.de:59125/api/tts?voice={0}";
        public TextToSpeechConfiguration Configuration { get; set; }

        protected string GetAudioFileDownloadUrl()
        {
            var ci = CultureInfo.GetCultureInfo(LanguageSettings.Instance.ActiveOrDefaultLanguage).Name.Replace("-", "_");
            ci = "de_DE"; //debug
            var voice = Configuration.Voice.ToLower() switch
            {
                "male" => $"{ci}/thorsten_low",
                "female" => $"{ci}/m-ailabs_low#ramona_deininger",
                _ => $"{ci}/m-ailabs_low#{Configuration.Voice}"
            };

            return string.Format(URL, voice);
        }

        protected IEnumerator DownloadAudio(string text, TaskCompletionSource<AudioClip> task)
        {
            var formData = Encoding.UTF8.GetBytes(text);
            using var request = UnityWebRequest.Post(GetAudioFileDownloadUrl(), "");
            request.uploadHandler = new UploadHandlerRaw(formData);

            // Request and wait for the response.
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var data = request.downloadHandler.data;

                if (data == null || data.Length == 0)
                {
                    Debug.LogError($"Error while retrieving audio: '{request.error}'");
                }

                var clip = new NAudioConverter().CreateAudioClipFromWAVE(data);
                task.SetResult(clip);
            }
            else
            {
                Debug.LogError($"Error while fetching audio from '{request.uri}' backend, error: '{request.error}'");
            }
        }

        public void SetConfig(TextToSpeechConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<AudioClip> ConvertTextToSpeech(string text)
        {
            var taskCompletion = new TaskCompletionSource<AudioClip>();
            CoroutineDispatcher.Instance.StartCoroutine(DownloadAudio(text, taskCompletion));

            return await taskCompletion.Task;
        }
    }
}