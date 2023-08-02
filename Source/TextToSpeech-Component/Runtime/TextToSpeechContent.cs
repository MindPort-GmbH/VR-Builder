using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.Configuration;

namespace VRBuilder.TextToSpeech.Audio
{
    /// <summary>
    /// Utility implementation of the <see cref="ITextToSpeechContent"/> interface that provides a default <see cref="IsCached"/> getter.
    /// </summary>
    public abstract class TextToSpeechContent : ITextToSpeechContent
    {
        /// <inheritdoc/>
        public abstract string Text { get; set; }

        /// <inheritdoc/>
        public virtual string LocalizationTable { get; set; }

        /// <inheritdoc/>
        public bool IsCached(Locale locale)
        {
            TextToSpeechConfiguration ttsConfiguration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();
            string filename = ttsConfiguration.GetUniqueTextToSpeechFilename(Text, locale);
            string filePath = $"{ttsConfiguration.StreamingAssetCacheDirectoryName}/{filename}";
            return File.Exists(Path.Combine(Application.streamingAssetsPath, filePath));
        }        
    }
}