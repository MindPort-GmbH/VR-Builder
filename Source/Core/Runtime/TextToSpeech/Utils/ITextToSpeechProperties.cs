using UnityEngine.Localization;

namespace VRBuilder.Core.TextToSpeech.Utils
{
    /// <summary>
    /// Interface for text-to-speech properties used to generate unique file names for audio files.
    /// Only non-null/non-empty fields are included in the generated filename.
    /// </summary>
    public interface ITextToSpeechProperties
    {
        /// <summary>
        /// Used key of the text-to-speech audio data.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Text content of the text-to-speech audio.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Language of the text-to-speech audio.
        /// </summary>
        Locale Locale { get; set; }

        /// <summary>
        /// Used localization table name of the text-to-speech audio data.
        /// </summary>
        string Table { get; set; }

        /// <summary>
        /// Used speaker of the text-to-speech audio.
        /// </summary>
        string Speaker { get; set; }

        /// <summary>
        /// Used format of the text-to-speech audio.
        /// </summary>
        TextToSpeechSettings.SupportedAudioType Format { get; set; }

        /// <summary>
        /// Sets the used key if localization is used returns the instance for chaining.
        /// </summary>
        /// <param name="key">The key value to set.</param>
        /// <returns>The instance for chaining.</returns>
        ITextToSpeechProperties WithKey(string key);

        /// <summary>
        /// Sets the text content of the text-to-speech file and returns the instance for chaining.
        /// </summary>
        /// <param name="text">The text value to set.</param>
        /// <returns>The instance for chaining.</returns>
        ITextToSpeechProperties WithText(string text);

        /// <summary>
        /// Sets the locale of the text-to-speech file and returns the instance for chaining.
        /// </summary>
        /// <param name="locale">The locale value to set.</param>
        /// <returns>The instance for chaining.</returns>
        ITextToSpeechProperties WithLocale(Locale locale);

        /// <summary>
        /// Sets the used localization table of the text-to-speech file and returns the instance for chaining.
        /// </summary>
        /// <param name="table">The table value to set.</param>
        /// <returns>The instance for chaining.</returns>
        ITextToSpeechProperties WithTable(string table);

        /// <summary>
        /// Sets the speaker of the text-to-speech file and returns the instance for chaining.
        /// </summary>
        /// <param name="speaker">The speaker value to set.</param>
        /// <returns>The instance for chaining.</returns>
        ITextToSpeechProperties WithSpeaker(string speaker);

        /// <summary>
        /// Sets the format of the text-to-speech file and returns the instance for chaining.
        /// </summary>
        /// <param name="format">The format value to set.</param>
        /// <returns>The instance for chaining.</returns>
        ITextToSpeechProperties WithFormat(TextToSpeechSettings.SupportedAudioType format);

        /// <summary>
        /// Generates the filename based on the properties using the factory's logic on this object.
        /// </summary>
        /// <returns>The generated filename.</returns>
        string ToFileName();
    }
}