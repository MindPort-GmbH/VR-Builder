// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: lgpl-3.0-or-later

using Source.TextToSpeech;
using VRBuilder.Core.TextToSpeech.Providers;

namespace VRBuilder.Core.TextToSpeech
{
    public class TextToSpeechService : ITextToSpeechService
    {
        public ITextToSpeechProvider DefaultOrActiveTextToSpeechProvider { get; set; } = new FileTextToSpeechProvider();

        public ITextToSpeechConfiguration Configuration
        {
            get => configuration;
            set => configuration = value;
        }

        private ITextToSpeechConfiguration configuration;

        public void SetConfiguration(ITextToSpeechConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
