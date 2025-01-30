using System.Security.Cryptography;
using System.Text;
using UnityEngine.Localization;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech.Utils
{
    public static class TextToSpeechUtils
    {
        /// <summary>
        /// Returns filename which uniquly identifies the audio by Backend, Language, Voice and also the text.
        /// </summary>
        public static string GetUniqueTextToSpeechFilename(this ITextToSpeechConfiguration configuration, string text, Locale locale, string format = "wav")
        {
            return $"TTS_{configuration.GetUniqueIdentifier(text, GetMd5Hash($"{text}"), locale)}.{format}";
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
        private static string GetMd5Hash(string input)
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
