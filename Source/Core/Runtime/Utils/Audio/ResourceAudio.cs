// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2024 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.Serialization;
using VRBuilder.Core.Localization;
using UnityEngine;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Localization;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.Core.Utils.Audio
{
    /// <summary>
    /// Unity resource-based audio data.
    /// </summary>
    [DataContract(IsReference = true)]
    [DisplayName("Play Audio File")]
    public class ResourceAudio : IAudioData, ILocalizedContent
    {
        private string path;

        /// <summary>
        /// File path relative to the Resources folder.
        /// </summary>
        [DataMember]
        [DisplayName("Resources path / Key")]
        [DisplayTooltip("The audio clip needs to be in a folder called Resources or one of its subfolders. The path to enter here is the relative path to the Resources folder, without extension. So, if the path is 'Assets/Resources/Audio/MyFile.wav', you would need to enter 'Audio/MyFile'.")]
        public string ResourcesPath
        {
            get => path;
            set
            {
                path = value;
                if (Application.isPlaying)
                {
                    Initialize();
                }
            }
        }

        public ResourceAudio(string path)
        {
            ResourcesPath = path;
        }

        protected ResourceAudio()
        {
            path = "";
        }

        /// <inheritdoc/>
        public bool HasAudio => AudioClip != null;

        /// <inheritdoc/>
        public IAudioClip AudioClip { get; private set; }

        /// <inheritdoc/>
        public bool IsReady => true;

        /// <inheritdoc/>
        public bool IsLoading => false;

        /// <inheritdoc/>
        public string ClipData
        {
            get => ResourcesPath;
            set => ResourcesPath = value;
        }

        public void Initialize()
        {
            AudioClip = null;

            if (string.IsNullOrEmpty(ResourcesPath))
            {
                ForwardingLogger.LogWarningFormat("Path to audio file is not defined.");
            }

            var clip = Resources.Load<AudioClip>(GetLocalizedContent());
            AudioClip = clip.ToAudioClipData();

            // Attempt to fallback to use the key as path.
            if (HasAudio == false)
            {
                clip = Resources.Load<AudioClip>(ResourcesPath);
                AudioClip = clip.ToAudioClipData();
            }

            if (HasAudio == false)
            {
                ForwardingLogger.LogWarningFormat("Given value '{0}' has returned no valid resource path for an audio clip, or it is not a valid resource path.", ResourcesPath);
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ResourcesPath);
        }

        public string GetLocalizedContent()
        {
            return ServiceRegistry.Get<ILanguageService>().GetLocalizedString(ResourcesPath);
        }
    }
}
