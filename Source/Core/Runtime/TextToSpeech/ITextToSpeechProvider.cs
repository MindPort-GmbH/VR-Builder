using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Utils;

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
        /// <param name="requestProperties">Properties containing all information about the text-to-speech audio data.</param>
        /// <returns>ready to play Audioclip</returns>
        Task<AudioClip> ConvertTextToSpeech(ITextToSpeechProperties requestProperties);

        /// <summary>
        /// Load config while editor- and runtime
        /// </summary>
        /// <returns>Returns configuration for the provider if successful</returns>
        public ITextToSpeechConfiguration LoadConfig();
    }
}
