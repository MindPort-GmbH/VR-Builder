// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: lgpl-3.0-or-later

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Source.TextToSpeech;
using UnityEngine;
using VRBuilder.Core.Localization;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.TextToSpeech.Utils;

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

        public string PrepareFilepathForTextToSpeechFile(ITextToSpeechFileLocator fileLocator)
        {
            string filename = fileLocator.WithTable(ServiceRegistry.Get<ILanguageService>().SelectedProcessLocalizationTable).ToFileName();
            string directory = Path.Combine(Application.temporaryCachePath.Replace('/', Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, ServiceRegistry.Get<ITextToSpeechService>().Configuration.StreamingAssetCacheDirectoryName);
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, filename);
        }
    }
}
