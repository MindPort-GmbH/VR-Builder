// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Primitives;

namespace VRBuilder.Core.Runtime.Utils
{
    /// <summary>
    /// In Unity we Map our AudioClip to Unity AudioClip
    /// </summary>
    public static class AudioClipExtensions
    {
        public static AudioClipData? ToAudioClipData(this AudioClip audioClip)
        {
            if (audioClip == null)
            {
                return null;
            }

            byte[] wavBytes = AudioUtility.AudioClipToWavBytes(audioClip);
            return new AudioClipData
            {
                RawAudioData = wavBytes,
                Frequency = audioClip.frequency,
                Channels = audioClip.channels,
                RawAudioClip = audioClip
            };
        }

        public static AudioClip ToUnity(this IAudioClip? audioClip, string clipName = "audio")
        {
            if (audioClip?.RawAudioData == null || audioClip.RawAudioData.Length == 0)
            {
                return null;
            }

            // TODO check if there is a better way
            return (AudioClip)audioClip.RawAudioClip;

            byte[] data = audioClip.RawAudioData;

            // Detect WAV format by header
            if (AudioUtility.IsWavFormat(data))
            {


            }

            // For other formats (MP3, OGG, etc.) use Unity's built-in decoder via a temp file.
            //TODO: return AudioUtility.CreateAudioClipFromUnknownBytes(data, clipName);
            return null;
        }
    }
}
