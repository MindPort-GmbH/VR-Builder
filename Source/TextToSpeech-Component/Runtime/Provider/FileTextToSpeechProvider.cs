using System;
using System.IO;
using System.Threading.Tasks;
using VRBuilder.Core.IO;
using UnityEngine;

namespace VRBuilder.TextToSpeech
{
    /// <summary>
    /// The disk based provider for text to speech, which is using the streaming assets folder.
    /// On the first step we check if the application has files provided on delivery.
    /// If there is no compatible file found, will download the file from the given
    /// fallback TextToSpeechProvider.
    /// </summary>
    public class FileTextToSpeechProvider : ITextToSpeechProvider
    {
        protected readonly ITextToSpeechProvider FallbackProvider;

        protected readonly IAudioConverter AudioConverter = new NAudioConverter();

        protected TextToSpeechConfiguration Configuration;

        public FileTextToSpeechProvider(ITextToSpeechProvider fallbackProvider, TextToSpeechConfiguration configuration)
        {
            Configuration = configuration;
            FallbackProvider = fallbackProvider;
        }

        public FileTextToSpeechProvider(ITextToSpeechProvider fallbackProvider, IAudioConverter audioConverter, TextToSpeechConfiguration configuration)
        {
            Configuration = configuration;
            AudioConverter = audioConverter;
            FallbackProvider = fallbackProvider;
        }

        /// <inheritdoc/>
        public async Task<AudioClip> ConvertTextToSpeech(string text)
        {
            string filename = Configuration.GetUniqueTextToSpeechFilename(text);
            string filePath = GetPathToFile(filename);
            AudioClip audioClip;
            
            if (IsFileCached(filePath))
            {
                byte[] bytes = GetCachedFile(filePath);
                float[] sound = TextToSpeechUtils.ShortsInByteArrayToFloats(bytes);
                
                audioClip = AudioClip.Create(text, channels: 1, frequency: 48000, lengthSamples: sound.Length, stream: false);
                audioClip.SetData(sound, 0);
            }
            else
            {
                audioClip = await FallbackProvider.ConvertTextToSpeech(text);

                if (Configuration.SaveAudioFilesToStreamingAssets)
                {
                    CacheAudio(audioClip, filePath);
                }
            }

            if (audioClip == null)
            {
                throw new CouldNotLoadAudioFileException("AudioClip is null.");
            }
            
            return audioClip;
        }

        /// <inheritdoc/>
        public void SetConfig(TextToSpeechConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Returns the relative location were the file is cached.
        /// </summary>
        protected virtual string GetPathToFile(string filename)
        {
            string directory = $"{Configuration.StreamingAssetCacheDirectoryName}/{filename}";
            return directory;
        }

        /// <summary>
        /// Stores given <paramref name="audioClip"/> in a cached directory.
        /// </summary>
        /// <remarks>When used in the Unity Editor the cached directory is inside the StreamingAssets folder; Otherwise during runtime the base path is the platform
        /// persistent data.</remarks>
        /// <param name="audioClip">The audio file to be cached.</param>
        /// <param name="filePath">Relative path where the <paramref name="audioClip"/> will be stored.</param>
        /// <returns>True if the file was successfully cached.</returns>
        protected virtual bool CacheAudio(AudioClip audioClip, string filePath)
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
        
            return AudioConverter.TryWriteAudioClipToFile(audioClip, absoluteFilePath);
        }

        /// <summary>
        /// Retrieves a cached file.
        /// </summary>
        /// <param name="filePath">Relative path where the cached file is stored.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        protected virtual byte[] GetCachedFile(string filePath)
        {
            return FileManager.Read(filePath);
        }

        /// <summary>
        /// Returns true is a file is cached in given relative <paramref name="filePath"/>.
        /// </summary>
        protected virtual bool IsFileCached(string filePath)
        {
            return FileManager.Exists(filePath);
        }
        
        public class CouldNotLoadAudioFileException : Exception
        {
            public CouldNotLoadAudioFileException(string msg) : base(msg) { }
            public CouldNotLoadAudioFileException(string msg, Exception ex) : base(msg, ex) { }
        }
    }
}