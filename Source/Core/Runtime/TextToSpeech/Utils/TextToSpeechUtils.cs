using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech.Utils
{
    public static class TextToSpeechUtils
    {
        /// <summary>
        /// Get GetUniqueIdentifier to identify the text relative to the locale and more properties.
        /// </summary>
        /// <param name="configuration">Used text-to-speech provider configuration.</param>
        /// <param name="audioData">Used audio data with meta-information.</param>
        /// <param name="locale">Used locale.</param>
        /// <returns>Returns a unique file name of the text-to-speech file.</returns>
        public static string GetUniqueTextToSpeechFilename(this ITextToSpeechConfiguration configuration, ITextToSpeechContent audioData, Locale locale)
        {
            return GetUniqueTextToSpeechFilename(configuration, new TextToSpeechFileProperties(audioData).WithLocale(locale));
        }

        /// <summary>
        /// Get GetUniqueIdentifier to identify the text relative to the locale and more properties.
        /// </summary>
        /// <param name="configuration">Used text-to-speech provider configuration.</param>
        /// <param name="key">Key of the string of the localization table.</param>
        /// <param name="text">The text to be checked if key is not set.</param>
        /// <param name="locale">Used locale.</param>
        /// <param name="speaker">Used speaker.</param>
        /// <returns>Returns a unique file name of the text-to-speech file.</returns>
        public static string GetUniqueTextToSpeechFilename(this ITextToSpeechConfiguration configuration, string key = "", string text = "", Locale locale = null, string speaker = "")
        {
            return GetUniqueTextToSpeechFilename(configuration, new TextToSpeechFileProperties().WithText(text).WithKey(key).WithLocale(locale).WithSpeaker(speaker));
        }

        /// <summary>
        /// Get GetUniqueIdentifier to identify the text relative to the locale and more properties.
        /// </summary>
        /// <param name="configuration">Used text-to-speech provider configuration.</param>
        /// <param name="fileProperties">Used text-to-speech file properties.</param>
        /// <returns>Returns a unique file name of the text-to-speech file.</returns>
        public static string GetUniqueTextToSpeechFilename(this ITextToSpeechConfiguration configuration, TextToSpeechFileProperties fileProperties)
        {
            return fileProperties.ToFileName();
        }

        /// <summary>
        /// Get a full path based on a <paramref name="text"/> to produce speech from, and create a directory for that.
        /// </summary>
        /// <param name="configuration">Current configuration</param>
        /// <param name="key">Key of the string of the localization table</param>
        /// <param name="text">The text to be checked if key is not set</param>
        /// <param name="locale">Used locale</param>
        /// <param name="speaker">Used speaker</param>
        /// <returns>True if the localizedContent in the chosen locale is cached</returns>
        public static string PrepareFilepathForText(this ITextToSpeechConfiguration configuration, string key, string text, Locale locale, string speaker = "")
        {
            string filename = configuration.GetUniqueTextToSpeechFilename(
                new TextToSpeechFileProperties()
                    .WithText(text)
                    .WithKey(key)
                    .WithLocale(locale)
                    .WithSpeaker(speaker)
                    .WithTable(RuntimeConfigurator.Instance.GetProcessStringLocalizationTable()));
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