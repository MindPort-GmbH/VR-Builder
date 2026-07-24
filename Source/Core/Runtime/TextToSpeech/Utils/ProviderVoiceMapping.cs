// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: lgpl-3.0-or-later

using System;

namespace VRBuilder.Core.TextToSpeech
{
    /// <summary>
    /// Mapping every voice with every <see cref="ITextToSpeechProvider"/> that supports voices.
    /// </summary>
    [Serializable]
    public class ProviderVoiceMapping
    {
        public string ProviderName;
        public string VoiceId;

        public ProviderVoiceMapping(string providerName, string voiceId)
        {
            ProviderName = providerName;
            VoiceId = voiceId;
        }
    }
}
