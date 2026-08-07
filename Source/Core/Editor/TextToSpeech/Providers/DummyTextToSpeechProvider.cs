// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Utils;
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
        public Task<IAudioClip> ConvertTextToSpeech(ITextToSpeechProperties textToSpeechProperties)
        {
            var audioClip = AudioClip.Create(textToSpeechProperties.Text, channels: 1, frequency: 48000, lengthSamples: 1, stream: false).ToAudioClipData();
            return Task.FromResult<IAudioClip>(audioClip);
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