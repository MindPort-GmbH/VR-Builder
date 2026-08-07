// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: lgpl-3.0-or-later

using System;
using VRBuilder.Core.TextToSpeech.Providers;

namespace VRBuilder.Core.TextToSpeech
{
    public static class TextToSpeechProviderExtensions
    {
        /// <summary>
        /// When the speech is generated in a separate tread, there are clicking sounds at the beginning and at the end of audio data.
        /// </summary>
        public static float[] RemoveArtifacts(this ITextToSpeechProvider provider, float[] floats)
        {
            // Empirically determined values.
            const int elementsToRemoveFromStart = 5000;
            const int elementsToRemoveFromEnd = 10000;

            float[] cleared = new float[floats.Length - elementsToRemoveFromStart - elementsToRemoveFromEnd];

            Array.Copy(floats, elementsToRemoveFromStart, cleared, 0, floats.Length - elementsToRemoveFromStart - elementsToRemoveFromEnd);

            return cleared;
        }

        /// <summary>
        /// The result comes in byte array, but there are actually short values inside (ranged from short.Min to short.Max).
        /// </summary>
        public static float[] ShortsInByteArrayToFloats(this ITextToSpeechProvider provider, byte[] shorts)
        {
            float[] floats = new float[shorts.Length / 2];

            for (int i = 0; i < floats.Length; i++)
            {
                short restoredShort = (short)(shorts[i * 2 + 1] << 8 | shorts[i * 2]);
                floats[i] = restoredShort / (float)short.MaxValue;
            }

            return floats;
        }
    }
}
