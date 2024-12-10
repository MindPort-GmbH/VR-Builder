using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.IO;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Utils;

namespace VRBuilder.Core.TextToSpeech.Providers
{
    /// <summary>
    /// The disk based provider for text to speech, which is using the streaming assets folder.
    /// On the first step we check if the application has files provided on delivery.
    /// If there is no compatible file found, will download the file from the given
    /// fallback TextToSpeechProvider.
    /// </summary>
    public class FileTextToSpeechProvider : ITextToSpeechProvider
    {
        protected ITextToSpeechConfiguration Configuration;

        public FileTextToSpeechProvider(ITextToSpeechConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<AudioClip> ConvertTextToSpeech(string text, Locale locale)
        {
            string filename = Configuration.GetUniqueTextToSpeechFilename(text, locale);
            string filePath = GetPathToFile(filename);
            AudioClip audioClip = null;

            if (await IsFileCached(filePath))
            {
                byte[] bytes = await GetCachedFile(filePath);
                float[] sound = TextToSpeechUtils.ShortsInByteArrayToFloats(bytes);

                audioClip = AudioClip.Create(text, channels: 1, frequency: 48000, lengthSamples: sound.Length, stream: false);
                audioClip.SetData(sound, 0);
            }
            else
            {
                Debug.Log($"No audio cached for TTS string. Audio will be generated in real time.");
                audioClip = await TextToSpeechProviderFactory.Instance.CreateProvider().ConvertTextToSpeech(text, locale);
            }

            if (audioClip == null)
            {
                throw new CouldNotLoadAudioFileException($"AudioClip is null for text '{text}'");
            }

            return audioClip;
        }

        public ITextToSpeechConfiguration LoadConfig()
        {
            return Configuration;
        }

        /// <inheritdoc/>
        public void SetConfig(ITextToSpeechConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Returns the relative location were the file is cached.
        /// </summary>
        protected virtual string GetPathToFile(string filename)
        {
            string directory = $"{RuntimeConfigurator.Configuration.GetTextToSpeechSettings().StreamingAssetCacheDirectoryName}/{filename}";
            return directory;
        }

        /// <summary>
        /// Retrieves a cached file.
        /// </summary>
        /// <param name="filePath">Relative path where the cached file is stored.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        protected virtual async Task<byte[]> GetCachedFile(string filePath)
        {
            if (Application.isPlaying)
            {
                return await FileManager.Read(filePath);
            }
            else
            {
                return File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, filePath));
            }
        }

        /// <summary>
        /// Returns true is a file is cached in given relative <paramref name="filePath"/>.
        /// </summary>
        protected virtual async Task<bool> IsFileCached(string filePath)
        {
            if (Application.isPlaying)
            {
                return await FileManager.Exists(filePath);
            }
            else
            {
                return File.Exists(Path.Combine(Application.streamingAssetsPath, filePath));
            }
        }

        public class CouldNotLoadAudioFileException : Exception
        {
            public CouldNotLoadAudioFileException(string msg) : base(msg)
            {
            }

            public CouldNotLoadAudioFileException(string msg, Exception ex) : base(msg, ex)
            {
            }
        }
    }
}
