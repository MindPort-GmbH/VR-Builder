// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System.Threading.Tasks;
using UnityEngine;

namespace VRBuilder.Core.Utils.Audio
{
    /// <summary>
    /// This class provides audio data in form of an AudioClip. Which also might not be loaded at the time needed,
    /// check first if there can be one provided.
    /// </summary>
    public interface IAudioData : ICanBeEmpty
    {
        /// <summary>
        /// Determines if the AudioSource has an AudioClip which can be played.
        /// </summary>
        bool HasAudioClip { get; }
        
        /// <summary>
        /// Returns true only when is busy loading an Audio Clip.
        /// </summary>
        /// <returns></returns>
        bool IsLoading { get; }

        /// <summary>
        /// Data used to retrieve the audio clip.
        /// </summary>
        string ClipData { get; set; }

        /// <summary>
        /// The AudioClip of this source, can be null. Best check first with HasAudio.
        /// </summary>
        AudioClip AudioClip { get; }

        /// <summary>
        /// Initializes the audio clip from the given data.
        /// </summary>
        Task InitializeAudioClip();
    }
}
