/******
/// this is a optional plugin and needs the Microsoft cognitive speech installed, which you can download here: https://aka.ms/csspeech/unitypackage
/// Due to limitations in assembly definition you have to set the MICROSOFT_COGNITIVE_SERVICES constraint manually in player settings after installation. 
****/ 
#if MICROSOFT_COGNITIVE_SERVICES
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
#endif