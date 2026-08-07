// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: lgpl-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRBuilder.Core.TextToSpeech
{
    /// <summary>
    /// Represents a voice profile that maps a display name to a specific voice ID for a TTS provider and language.
    /// </summary>
    [Serializable]
    public class VoiceProfile
    {
        /// <summary>
        /// Display name of the profile (e.g., "Marcello", "Markus").
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// ISO language code (e.g., "de-DE", "en-US").
        /// </summary>
        public string[] LanguageCode;

        /// <summary>
        /// Mappings from provider names to voice IDs.
        /// </summary>
        public List<ProviderVoiceMapping> providerVoiceMappings;

        /// <summary>
        /// Fallback provider if there is no avaibled provider for multiple voices
        /// </summary>
        private string fallbackProviderName;

        public List<ProviderVoiceMapping> ProviderVoiceMappings
        {
            get => providerVoiceMappings;
            set => providerVoiceMappings = value;
        }

        public VoiceProfile()
        {
            DisplayName = "New Profile";
            LanguageCode = new []{"all"};
            providerVoiceMappings = new List<ProviderVoiceMapping>();
        }

        public VoiceProfile(string displayName, string[] languageCode, string voiceId, string[] providerNames)
        {
            DisplayName = displayName;
            LanguageCode = languageCode;
            providerVoiceMappings = providerNames.Select(p => new ProviderVoiceMapping(p, voiceId)).ToList();
        }
    }
}
