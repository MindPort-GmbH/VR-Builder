using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;

namespace VRBuilder.Core.Editor.TextToSpeech.Providers
{
    /// <summary>
    /// Dummy provider that creates empty files, useful in case of compatibility issues.
    /// </summary>
    public class DummyTextToSpeechProvider : ITextToSpeechProvider
    {
        /// <inheritdoc/>
        public Task<AudioClip> ConvertTextToSpeech(string key, string text, Locale locale, string speaker)
        {
            AudioClip audioClip = AudioClip.Create(text, channels: 1, frequency: 48000, lengthSamples: 1, stream: false);

            return Task.FromResult(audioClip);
        }

        /// <inheritdoc />
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