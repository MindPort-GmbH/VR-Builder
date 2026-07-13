// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;
using Core.Runtime.Utils;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.Primitives;
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
        public Task<IAudioClip> ConvertTextToSpeech(string key, string text, Locale locale, string speaker)
        {
            var audioClip = AudioClip.Create(text, channels: 1, frequency: 48000, lengthSamples: 1, stream: false).ToAudioClipData();
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