// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using UnityEngine;

namespace VRBuilder.Core.Audio
{
    /// <summary>
    /// Unity resource based audio data.
    /// </summary>
    [DataContract(IsReference = true)]
    [DisplayName("Play Audio File")]
    public class ResourceAudio : IAudioData
    {
        private string path;

        [DataMember]
        public string ResourcesPath
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                InitializeAudioClip();
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

        public bool HasAudioClip
        {
            get
            {
                return AudioClip != null;
            }
        }

        public AudioClip AudioClip { get; private set; }

        private void InitializeAudioClip()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarningFormat("Path to audio file is not defined.");
                return;
            }

            AudioClip = Resources.Load<AudioClip>(path);

            if (HasAudioClip == false)
            {
                Debug.LogWarningFormat("Given path '{0}' to resource has returned no audio clip", path);
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ResourcesPath);
        }
    }
}
