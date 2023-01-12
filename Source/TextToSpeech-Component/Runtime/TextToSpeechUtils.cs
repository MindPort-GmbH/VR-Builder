using System.Text;
using System.Security.Cryptography;
using VRBuilder.Core.Internationalization;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using VRBuilder.TextToSpeech.Audio;
using VRBuilder.Core.Configuration;
using System.Threading.Tasks;

namespace VRBuilder.TextToSpeech
{
    public static class TextToSpeechUtils
    {
        private static List<TextToSpeechAudio> registeredAudio = new List<TextToSpeechAudio>();

        public static void RegisterClip(TextToSpeechAudio audio) 
        { 
            if(registeredAudio.Contains(audio) == false)
            {
                registeredAudio.Add(audio);
            }
        }

        /// <summary>
        /// Returns filename which uniquly identifies the audio by Backend, Language, Voice and also the text.
        /// </summary>
        public static string GetUniqueTextToSpeechFilename(this TextToSpeechConfiguration configuration, string text, string format = "wav")
        {
            string hash = string.Format("{0}_{1}", configuration.Voice, text);
            return string.Format(@"TTS_{0}_{1}_{2}.{3}", configuration.Provider, LanguageSettings.Instance.ActiveOrDefaultLanguage, GetMd5Hash(hash).Replace("-", ""), format);
        }
        
        /// <summary>
        /// The result comes in byte array, but there are actually short values inside (ranged from short.Min to short.Max).
        /// </summary>
        public static float[] ShortsInByteArrayToFloats(byte[] shorts)
        {
            float[] floats = new float[shorts.Length / 2];

            for (int i = 0; i < floats.Length; i++)
            {
                short restoredShort = (short) ((shorts[i * 2 + 1] << 8) | (shorts[i * 2]));
                floats[i] = restoredShort / (float) short.MaxValue;
            }

            return floats;
        }
        
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



        public static async Task CacheAudioClip(string text, TextToSpeechConfiguration configuration)
        {
            string filename = configuration.GetUniqueTextToSpeechFilename(text);
            string filePath = $"{configuration.StreamingAssetCacheDirectoryName}/{filename}";

            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider(configuration);
            AudioClip audioClip = await provider.ConvertTextToSpeech(text);

            CacheAudio(audioClip, filePath, new NAudioConverter()); // TODO expose converter to configuration
        }

        /// <summary>
        /// Stores given <paramref name="audioClip"/> in a cached directory.
        /// </summary>
        /// <remarks>When used in the Unity Editor the cached directory is inside the StreamingAssets folder; Otherwise during runtime the base path is the platform
        /// persistent data.</remarks>
        /// <param name="audioClip">The audio file to be cached.</param>
        /// <param name="filePath">Relative path where the <paramref name="audioClip"/> will be stored.</param>
        /// <returns>True if the file was successfully cached.</returns>
        private static bool CacheAudio(AudioClip audioClip, string filePath, IAudioConverter converter)
        {
            // Ensure target directory exists.
            string fileName = Path.GetFileName(filePath);
            string relativePath = Path.GetDirectoryName(filePath);

            string basedDirectoryPath = Application.isEditor ? Application.streamingAssetsPath : Application.persistentDataPath;
            string absolutePath = Path.Combine(basedDirectoryPath, relativePath);

            if (string.IsNullOrEmpty(absolutePath) == false && Directory.Exists(absolutePath) == false)
            {
                Directory.CreateDirectory(absolutePath);
            }

            string absoluteFilePath = Path.Combine(absolutePath, fileName);

            return converter.TryWriteAudioClipToFile(audioClip, absoluteFilePath);
        }

        public static async void CacheAllClips()
        {
            TextToSpeechConfiguration configuration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();

            foreach(TextToSpeechAudio audio in registeredAudio)
            {
                if (string.IsNullOrEmpty(audio.Text) == false)
                {
                    await CacheAudioClip(audio.Text, configuration);
                }
            }
        }
    }
}