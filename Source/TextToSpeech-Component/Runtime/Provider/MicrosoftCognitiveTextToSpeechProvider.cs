using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;

namespace VRBuilder.TextToSpeech
{
    ///<author email="a.schaub@lefx.de">Aron Schaub</author>
    public class MicrosoftCognitiveTextToSpeechProvider : ITextToSpeechProvider
    {
        private SpeechSynthesizer _client;
        public string regionKey = ""; 

        public void SetConfig(TextToSpeechConfiguration configuration)
        {
            var config = SpeechConfig.FromSubscription(configuration.Auth, regionKey);
            _client = new SpeechSynthesizer(config, null);
        }

        public async Task<AudioClip> ConvertTextToSpeech(string text)
        {
            var speakTextAsync = await _client.SpeakTextAsync(text);
            var clip = new NAudioConverter().CreateAudioClipFromWAVE(speakTextAsync.AudioData);
            return clip;
        }
    }
}