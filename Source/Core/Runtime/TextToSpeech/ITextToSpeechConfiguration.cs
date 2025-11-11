using UnityEngine.Localization;

namespace VRBuilder.Core.TextToSpeech.Configuration
{
    /// <summary>
    /// Base interface to implement a new text to speech configuration
    /// </summary>
    /// <remarks>
    /// It always should implement with SettingsObject to prevent type cast errors
    /// </remarks>
    public interface ITextToSpeechConfiguration
    {
    }
}