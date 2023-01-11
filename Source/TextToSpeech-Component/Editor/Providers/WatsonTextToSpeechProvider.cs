using VRBuilder.Core.Internationalization;
using UnityEngine;
using UnityEngine.Networking;

namespace VRBuilder.TextToSpeech
{
    /// <summary>
    /// Uses the Watson text to speech api
    /// </summary>
    public class WatsonTextToSpeechProvider : WebTextToSpeechProvider
    {
        private const string URL = "https://stream.watsonplatform.net/text-to-speech/api/v1/synthesize?text={0}&voice={1}_{2}&accept=audio/mp3";

        public WatsonTextToSpeechProvider() : base() { }

        public WatsonTextToSpeechProvider(UnityWebRequest unityWebRequest) : base(unityWebRequest) { }

        protected override UnityWebRequest CreateRequest(string url, string text)
        {
            UnityWebRequest request = base.CreateRequest(url, text);
            request.SetRequestHeader("Authorization", Configuration.Auth);
            
            return request;
        }

        protected override string GetAudioFileDownloadUrl(string text)
        {
            return string.Format(URL, text, LanguageSettings.Instance.ActiveOrDefaultLanguage, Configuration.Voice);
        }
    }
}