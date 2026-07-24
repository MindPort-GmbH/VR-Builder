// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading.Tasks;
using Source.TextToSpeech;
using UnityEngine;
using VRBuilder.Core.IO;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Runtime.Utils;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Utils;

namespace VRBuilder.Core.TextToSpeech.Providers
{
    /// <summary>
    /// The disk-based provider for text to speech, which is using the streaming assets folder.
    /// On the first step we check if the application has files provided on delivery.
    /// If there is no compatible file found, will download the file from the given
    /// fallback TextToSpeechProvider.
    /// </summary>
    public class FileTextToSpeechProvider : ITextToSpeechProvider
    {
        protected ITextToSpeechProviderConfiguration providerConfiguration = new FileTextToSpeechProviderConfiguration();

        /// <inheritdoc/>
        public async Task<IAudioClip> ConvertTextToSpeech(ITextToSpeechFileLocator textToSpeechFileLocator)
        {
            string filename = textToSpeechFileLocator.ToFileName();
            string filePath = GetPathToFile(filename);
            IAudioClip? audioClip = null;

            if (await IsFileCached(filePath))
            {
                byte[] bytes = await GetCachedFile(filePath);
                float[] sound = this.ShortsInByteArrayToFloats(bytes);

                int sampleRate = BitConverter.ToInt32(bytes, 24);
                var ac = AudioClip.Create(textToSpeechFileLocator.Text, channels: 1, frequency: sampleRate, lengthSamples: sound.Length, stream: false);
                ac.SetData(sound, 0);
                audioClip = ac.ToAudioClipData();
            }
            else
            {
                ForwardingLogger.Log($"No audio cached for TTS string. File {filePath} not found. Audio will be generated in real time.");
                audioClip = await ServiceRegistry.Get<ITextToSpeechService>().DefaultOrActiveTextToSpeechProvider.ConvertTextToSpeech(textToSpeechFileLocator);
            }

            if (audioClip == null)
            {
                throw new CouldNotLoadAudioFileException($"AudioClip is null for text '{textToSpeechFileLocator.Text}'");
            }

            return audioClip;
        }

        /// <inheritdoc />
        public ITextToSpeechProviderConfiguration LoadConfig()
        {
            return providerConfiguration;
        }

        /// <inheritdoc/>
        public void SetConfig(ITextToSpeechProviderConfiguration providerConfiguration)
        {
            this.providerConfiguration = providerConfiguration;
        }

        /// <summary>
        /// Returns the relative location were the file is cached.
        /// </summary>
        protected virtual string GetPathToFile(string filename)
        {
            string directory = $"{ServiceRegistry.Get<ITextToSpeechService>().Configuration.StreamingAssetCacheDirectoryName}/{filename}";
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
                return await ServiceRegistry.Get<IPlatformFileSystem>().Read(filePath);
            }
            return await File.ReadAllBytesAsync(Path.Combine(Application.streamingAssetsPath, filePath));
        }

        /// <summary>
        /// Returns true is a file is cached in given relative <paramref name="filePath"/>.
        /// </summary>
        protected virtual async Task<bool> IsFileCached(string filePath)
        {
            if (Application.isPlaying)
            {
                return await ServiceRegistry.Get<IPlatformFileSystem>().Exists(filePath);
            }

            return File.Exists(Path.Combine(Application.streamingAssetsPath, filePath));
        }

        /// <summary>
        /// Exception for not loaded audio files if the path is wrong or the file is not generated.
        /// </summary>
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
