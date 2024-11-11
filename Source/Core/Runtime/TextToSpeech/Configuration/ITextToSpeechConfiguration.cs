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
        /// <summary>
        /// Get GetUniqueIdentifier to identify the text relative to the locale and hash value
        /// </summary>
        /// <param name="text">Text to get the identifier</param>
        /// <param name="md5Hash">Hashed text value</param>
        /// <param name="locale">Used locale</param>
        /// <returns>A unique identifier of the text</returns>
        string GetUniqueIdentifier(string text, string md5Hash, Locale locale);

        /// <summary>
        /// Check if the localizedContent in the chosen locale is cached
        /// </summary>
        /// <param name="locale">Used locale</param>
        /// <param name="localizedContent">Content to be checked</param>
        /// <returns>True if the localizedContent in the chosen locale is cached</returns>
        bool IsCached(Locale locale, string localizedContent);
    }
}
