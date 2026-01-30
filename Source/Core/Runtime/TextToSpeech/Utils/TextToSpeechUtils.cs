using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech.Utils
{
    public static class TextToSpeechUtils
    {

        /// <summary>
        /// Get GetUniqueIdentifier to identify the text relative to the locale and hash value
        /// </summary>
        /// <param name="configuration">Used text-to-speech provider configuration</param>
        /// <param name="key">Key of the string of the localization table</param>
        /// <param name="text">The text to be checked if key is not set</param>
        /// <param name="locale">Used locale</param>
        /// <param name="format">Used file format</param>
        /// <returns></returns>
        public static string GetUniqueTextToSpeechFilename(this ITextToSpeechConfiguration configuration, string key, string text, Locale locale, string format = "wav")
        {
            return !LocalizationSettings.HasSettings || string.IsNullOrEmpty(key)
                ? $"TTS_{locale.Identifier.Code}_{GetMd5Hash(text).Replace("-", "")}"
                : $"TTS_{RuntimeConfigurator.Instance.GetProcessStringLocalizationTable()}_{key}_{locale.Identifier.Code}.{format}";
        }

        /// <summary>
        /// Get a full path based on a <paramref name="text"/> to produce speech from, and create a directory for that.
        /// </summary>
        /// <param name="configuration">Current configuration</param>
        /// <param name="key">Key of the string of the localization table</param>
        /// <param name="text">The text to be checked if key is not set</param>
        /// <param name="locale">Used locale</param>
        /// <returns>True if the localizedContent in the chosen locale is cached</returns>
        public static string PrepareFilepathForText(this ITextToSpeechConfiguration configuration, string key, string text, Locale locale)
        {
            string filename = configuration.GetUniqueTextToSpeechFilename(key, text, locale);
            string directory = Path.Combine(Application.temporaryCachePath.Replace('/', Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, RuntimeConfigurator.Configuration.GetTextToSpeechSettings().StreamingAssetCacheDirectoryName);
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, filename);
        }

        /// <summary>
        /// When the speech is generated in a separate tread, there are clicking sounds at the beginning and at the end of audio data.
        /// </summary>
        public static float[] RemoveArtifacts(float[] floats)
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
        public static float[] ShortsInByteArrayToFloats(byte[] shorts)
        {
            float[] floats = new float[shorts.Length / 2];

            for (int i = 0; i < floats.Length; i++)
            {
                short restoredShort = (short)(shorts[i * 2 + 1] << 8 | shorts[i * 2]);
                floats[i] = restoredShort / (float)short.MaxValue;
            }

            return floats;
        }

        /// <summary>
        /// Hashed the input string
        /// </summary>
        /// <param name="input">Input string that has to be hashed</param>
        /// <returns>Hashed input as MD5 Hash</returns>
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(input);

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(buffer);
                // Create a new StringBuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();
                // Loop through each byte of the hashed data and format each one as a hexadecimal string.
                foreach (byte @byte in data)
                {
                    sBuilder.Append(@byte.ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }
}