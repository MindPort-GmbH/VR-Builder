// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using UnityEngine;

namespace Core.Runtime.Utils
{
    public static class AudioUtility
    {
        public static byte[] FloatArrayToWavBytes(float[] samples, int sampleRate, int channels)
        {
            var sampleCount = samples.Length;
            var byteRate = sampleRate * channels * 2; // 16-bit
            var dataSize = sampleCount * 2;

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                // RIFF header
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataSize);
                writer.Write(new[] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                writer.Write(new[] { 'f', 'm', 't', ' ' });
                writer.Write(16); // chunk size
                writer.Write((short)1); // PCM format
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)(channels * 2)); // block align
                writer.Write((short)16); // bits per sample

                // data chunk
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataSize);

                for (var i = 0; i < sampleCount; i++)
                {
                    writer.Write((short)(samples[i] * short.MaxValue));
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        public static byte[] AudioClipToWavBytes(AudioClip clip)
        {
            if (clip == null)
            {
                return Array.Empty<byte>();
            }

            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            return FloatArrayToWavBytes(samples, clip.frequency, clip.channels);
        }

        public static bool IsWavFormat(byte[] data) => data is { Length: >= 4 } && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F';
    }
}