using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using VRBuilder.Core.Internationalization;
using VRBuilder.Unity;

namespace VRBuilder.TextToSpeech
{
    public class Mimic3TextToSpeechProvider : ITextToSpeechProvider
    {
        private const string URL = "http://localhost:59125";
        public TextToSpeechConfiguration Configuration { get; set; }

        protected string GetAudioFileDownloadUrl()
        {
            // string lang = LanguageSettings.Instance.ActiveOrDefaultLanguage.ToLower();
            string voice = Configuration.Voice;
            if (string.IsNullOrEmpty(Configuration.URL))
                Configuration.URL = URL;
            return $"{Configuration.URL}/api/tts?voice={voice}";
        }

        protected IEnumerator DownloadAudio(string text, TaskCompletionSource<AudioClip> task)
        {
            byte[] formData = Encoding.UTF8.GetBytes(text);
            using var request = UnityWebRequest.Post(GetAudioFileDownloadUrl(), "");
            request.uploadHandler = new UploadHandlerRaw(formData);

            // Request and wait for the response.
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] data = request.downloadHandler.data;

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
            TaskCompletionSource<AudioClip> taskCompletion = new TaskCompletionSource<AudioClip>();
            CoroutineDispatcher.Instance.StartCoroutine(DownloadAudio(text, taskCompletion));

            return await taskCompletion.Task;
        }
    }

    public class Properties
    {
        [JsonProperty("length_scale")]
        public double LengthScale { get; set; }

        [JsonProperty("noise_scale")]
        public double NoiseScale { get; set; }

        [JsonProperty("noise_w")]
        public double NoiseW { get; set; }
    }

    public class Root
    {
        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("language_english")]
        public string LanguageEnglish { get; set; }

        [JsonProperty("language_native")]
        public string LanguageNative { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("sample_text")]
        public string SampleText { get; set; }

        [JsonProperty("speakers")]
        public List<string> Speakers { get; set; }

        [JsonProperty("version")]
        public object Version { get; set; }
    }
}