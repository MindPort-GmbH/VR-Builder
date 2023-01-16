using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.EntityOwners;
using VRBuilder.TextToSpeech;
using VRBuilder.TextToSpeech.Audio;
using System.Linq;

namespace VRBuilder.Editor.TextToSpeech
{
    public static class TextToSpeechEditorUtils
    {
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

            foreach (TextToSpeechAudio clip in configuration.RegisteredClips) 
            {
                if (string.IsNullOrEmpty(clip.Text) == false)
                {
                    await CacheAudioClip(clip.Text, configuration);
                }
            }
        }
    }
}