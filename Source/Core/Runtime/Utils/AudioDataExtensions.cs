// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Utils.Audio;

namespace Core.Runtime.Utils
{
    /// <summary>
    /// In Unity we Map our AudioData to AudioSource.
    /// </summary>
    public static class AudioDataExtensions
    {
        public static AudioData? ToAudioData(this AudioSource audioSource)
        {
            if (audioSource == null || audioSource.clip == null)
                return null;

            AudioClipData? clipData = audioSource.clip.ToAudioClipData();
            return clipData.HasValue ? new AudioData(clipData.Value, audioSource.clip.name) : null;
        }

        public static AudioSource ToUnity(this IAudioData? audioData, GameObject host, string clipName = "audio")
        {
            var source = host.AddComponent<AudioSource>();
            if (audioData != null)
                source.clip = audioData.AudioClip.ToUnity(clipName);
            else
                ForwardingLogger.LogWarning("AudioData is null");
            return source;
        }
    }
}