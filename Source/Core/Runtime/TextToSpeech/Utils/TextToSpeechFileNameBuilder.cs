using System.Globalization;
using Source.TextToSpeech;
using VRBuilder.Core.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Runtime.Registry;
using static VRBuilder.Core.TextToSpeech.Utils.ITextToSpeechConfigurationExtension;

namespace VRBuilder.Core.TextToSpeech.Utils
{
	/// <summary>
	/// Helper class for creating unique file names for text-to-speech audio files.
	/// Only non-null/non-empty fields are included in the generated filename.
	/// </summary>
	public class TextToSpeechFileNameBuilder : ITextToSpeechFileNameBuilder
	{
		/// <summary>
		/// Used key of the text-to-speech audio data.
		/// </summary>
		public string Key { get; set; } = "";

		/// <summary>
		/// Text content of the text-to-speech audio.
		/// </summary>
		public string Text { get; set; } = "";

		/// <summary>
		/// Language of the text-to-speech audio.
		/// </summary>
		public CultureInfo Locale { get; set; } = null;

		/// <summary>
		/// Used localization table name of the text-to-speech audio data.
		/// </summary>
		public string Table { get; set; } = "";

		/// <summary>
		/// Used speaker of the text-to-speech audio.
		/// </summary>
		public string Speaker { get; set; } = "";

		/// <summary>
		/// Used format of the text-to-speech audio.
		/// </summary>
		public ITextToSpeechConfiguration.SupportedAudioType Format { get; set; } = ITextToSpeechConfiguration.SupportedAudioType.WAV;

		/// <summary>
        /// Sets the key property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechFileNameBuilder WithKey(string key)
        {
            Key = key;
            return this;
        }

        /// <summary>
        /// Sets the text property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechFileNameBuilder WithText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets the locale property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechFileNameBuilder WithLocale(CultureInfo locale)
        {
            Locale = locale;
            return this;
        }

        /// <summary>
        /// Sets the table property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechFileNameBuilder WithTable(string table)
        {
	        if (!string.IsNullOrEmpty(Table))
	        {
		        ForwardingLogger.LogWarning($"Already set table '${Table}' will be overwritten with new table '{table}'.");
	        }
            Table = table;
            return this;
        }

        /// <summary>
        /// Sets the speaker property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechFileNameBuilder WithSpeaker(string speaker)
        {
            Speaker = speaker;
            return this;
        }

        /// <summary>
        /// Sets the format property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechFileNameBuilder WithFormat(ITextToSpeechConfiguration.SupportedAudioType format)
        {
            Format = format;
            return this;
        }

        /// <summary>
        /// Create a new property to generate the unique file name of the text-to-speech file
        /// </summary>
        /// <param name="audioData">Used audio information for basic unique filename</param>
        public TextToSpeechFileNameBuilder(ITextToSpeechContent audioData = null)
        {
	        if (audioData is not null)
	        {
		        Text = audioData.Text;
		        Speaker = audioData.Speaker;
	        }
        }

        /// <summary>
        /// Generates the filename based on the properties using the factory's logic on this object.
        /// </summary>
        public string ToFileName()
        {
	        return ToFileName(this);
        }

        /// <summary>
        /// Generates the filename based on the properties using the factory's logic.
        /// </summary>
        ///
        public static string ToFileName(TextToSpeechFileNameBuilder fileNameBuilder)
        {
            // If key is empty or localization isn't available, use the simpler format
            if (string.IsNullOrEmpty(fileNameBuilder.Key) || !LocalizationSettings.HasSettings)
            {
                return $"TTS_{(fileNameBuilder.Speaker != "" ? $"{fileNameBuilder.Speaker}_" : "")}{fileNameBuilder.Locale?.ToString() ?? ServiceRegistry.Get<ILanguageService>().ActiveOrDefaultRegionCode}_" +
                       $"{GetMd5Hash(fileNameBuilder.Text).Replace("-", "")}." +
                       $"{TextToSpeechProviderSettings.GetFileTypeName(TextToSpeechProviderSettings.Instance?.SelectedAudioType ?? ITextToSpeechConfiguration.SupportedAudioType.WAV)}";
            }

            // Otherwise use the full format with table and key
            // Speaker_LocalisationTable_Key_Locale_TextHash.Type
            return $"TTS_{(fileNameBuilder.Speaker != "" ? $"{fileNameBuilder.Speaker}_" : "")}" +
                   $"{(string.IsNullOrEmpty(fileNameBuilder.Table)? ServiceRegistry.Get<ILanguageService>().ProcessStringLocalizationTable: fileNameBuilder.Table)}_" +
                   $"{fileNameBuilder.Key}_" +
                   $"{(fileNameBuilder.Locale is null? ServiceRegistry.Get<ILanguageService>().ActiveOrDefaultRegionCode : fileNameBuilder.Locale.ToString())}_" +
                   $"{GetMd5Hash(fileNameBuilder.Text).Replace("-", "")}." +
                   $"{TextToSpeechProviderSettings.GetFileTypeName(TextToSpeechProviderSettings.Instance?.SelectedAudioType ?? ITextToSpeechConfiguration.SupportedAudioType.WAV)}";
        }
	}
}
