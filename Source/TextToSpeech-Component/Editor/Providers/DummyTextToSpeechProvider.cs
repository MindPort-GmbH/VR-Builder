using System.Threading.Tasks;
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

        /// <inheritdoc/>
        public void SetConfig(TextToSpeechConfiguration configuration)
        {
        }
    }
}