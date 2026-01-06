using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.TextToSpeech.Configuration;

namespace VRBuilder.Core.TextToSpeech.Providers
{
    /// <summary>
    /// TextToSpeechProvider allows converting text to AudioClips.
    /// </summary>
    public interface ITextToSpeechProvider
    {
        /// <summary>
        /// Used for setting the config file.
        /// </summary>
        void SetConfig(ITextToSpeechConfiguration configuration);

        /// <summary>
        /// Loads the AudioClip file for the given text.
        /// </summary>
        /// <param name="key">unique identifier of the original text can be either LanguageTable key or md5hash of untranslated text</param>
        /// <param name="text">translated text</param>
        /// <param name="locale">locale of translated text</param>
        /// <returns>ready to play Audioclip</returns>
        Task<AudioClip> ConvertTextToSpeech(string key, string text, Locale locale);

        /// <summary>
        /// Load config while editor- and runtime
        /// </summary>
        /// <returns>Returns configuration for the provider if successful</returns>
        public ITextToSpeechConfiguration LoadConfig();

        /// <summary>
        /// If the text-to-speech provider supports multiple speakers
        /// </summary>
        /// <returns>True if this feature is supported</returns>
        public bool SupportsMultiSpeaker();
    }
}
