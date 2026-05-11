using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.TextToSpeech.Utils;

namespace VRBuilder.Core.Editor.TextToSpeech.Providers
{
    /// <summary>
    /// Dummy provider that creates empty files, useful in case of compatibility issues.
    /// </summary>
    public class DummyTextToSpeechProvider : ITextToSpeechProvider
    {
        /// <inheritdoc/>
        public Task<AudioClip> ConvertTextToSpeech(ITextToSpeechProperties textToSpeechProperties)
        {
            AudioClip audioClip = AudioClip.Create(textToSpeechProperties.Text, channels: 1, frequency: 48000, lengthSamples: 1, stream: false);

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