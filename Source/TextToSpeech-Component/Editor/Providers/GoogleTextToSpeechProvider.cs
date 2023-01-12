using VRBuilder.Core.Internationalization;
using UnityEngine.Networking;
using VRBuilder.TextToSpeech;

namespace VRBuilder.Editor.TextToSpeech
{
    /// <summary>
    /// Uses the Google text to speech api
    /// </summary>
    public class GoogleTextToSpeechProvider : WebTextToSpeechProvider
    {
        private const string URL = "https://www.google.com/speech-api/v1/synthesize?ie=UTF-8&text={0}&lang={1}&sv={2}&vn=rjs&speed=0.4";

        public GoogleTextToSpeechProvider() : base() { }

        public GoogleTextToSpeechProvider(UnityWebRequest unityWebRequest) : base(unityWebRequest) { }

        public GoogleTextToSpeechProvider(UnityWebRequest unityWebRequest, IAudioConverter audioConverter) : base(unityWebRequest, audioConverter) { }

        protected override string GetAudioFileDownloadUrl(string text)
        {
            return string.Format(URL, text, LanguageSettings.Instance.ActiveOrDefaultLanguage, Configuration.Voice);
        }
    }
}