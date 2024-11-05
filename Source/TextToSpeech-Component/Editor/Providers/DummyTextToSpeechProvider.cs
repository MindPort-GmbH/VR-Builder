using System.Threading.Tasks;
using Source.TextToSpeech_Component.Runtime;
using UnityEngine;
using VRBuilder.TextToSpeech;
using UnityEngine.Localization; 

namespace VRBuilder.Editor.TextToSpeech
{
    /// <summary>
    /// Dummy provider that creates empty files, useful in case of compatibility issues.
    /// </summary>
    public class DummyTextToSpeechProvider : ITextToSpeechProvider
    {
        /// <inheritdoc/>
        public Task<AudioClip> ConvertTextToSpeech(string text, Locale locale)
        {
            AudioClip audioClip = AudioClip.Create(text, channels: 1, frequency: 48000, lengthSamples: 1, stream: false);

            return Task.FromResult(audioClip);
        }

        public ITextToSpeechConfiguration LoadConfig()
        {
            return null;
        }

        /// <inheritdoc/>
        public void SetConfig(ITextToSpeechConfiguration configuration)
        {
        }
    }
}