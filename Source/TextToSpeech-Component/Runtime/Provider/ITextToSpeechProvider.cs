using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.Runtime.Utils;

namespace VRBuilder.TextToSpeech
{
    /// <summary>
    /// TextToSpeechProvider allows to convert text to AudioClips.
    /// </summary>
    public interface ITextToSpeechProvider
    {
        /// <summary>
        /// Used for setting the config file.
        /// </summary>
        void SetConfig(MicrosoftTextToSpeechConfiguration configuration);

        /// <summary>
        /// Loads the AudioClip file for the given text.
        /// </summary>
        Task<AudioClip> ConvertTextToSpeech(string text, Locale locale);
        
        public ScriptableObject LoadConfig();
    }
}