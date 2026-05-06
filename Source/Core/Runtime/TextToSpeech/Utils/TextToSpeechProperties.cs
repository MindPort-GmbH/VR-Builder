using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Settings;
using static VRBuilder.Core.TextToSpeech.Utils.TextToSpeechUtils;

namespace VRBuilder.Core.TextToSpeech.Utils
{
	/// <summary>
	/// Helper class for creating unique file names for text-to-speech audio files.
	/// Only non-null/non-empty fields are included in the generated filename.
	/// </summary>
	public class TextToSpeechProperties : ITextToSpeechProperties
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
		public Locale Locale { get; set; } = null;

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
		public TextToSpeechSettings.SupportedAudioType Format { get; set; } = TextToSpeechSettings.SupportedAudioType.WAV;
		
		/// <summary>
        /// Sets the key property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechProperties WithKey(string key)
        {
            Key = key;
            return this;
        }

        /// <summary>
        /// Sets the text property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechProperties WithText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets the locale property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechProperties WithLocale(Locale locale)
        {
            Locale = locale;
            return this;
        }

        /// <summary>
        /// Sets the table property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechProperties WithTable(string table)
        {
	        if (!string.IsNullOrEmpty(Table))
	        {
		        Debug.LogWarning($"Already set table '${Table}' will be overwritten with new table '{table}'.");
	        }
            Table = table;
            return this;
        }

        /// <summary>
        /// Sets the speaker property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechProperties WithSpeaker(string speaker)
        {
            Speaker = speaker;
            return this;
        }

        /// <summary>
        /// Sets the format property and returns the instance for chaining.
        /// </summary>
        public ITextToSpeechProperties WithFormat(TextToSpeechSettings.SupportedAudioType format)
        {
            Format = format;
            return this;
        }

        /// <summary>
        /// Create a new property to generate the unique file name of the text-to-speech file
        /// </summary>
        /// <param name="audioData">Used audio information for basic unique filename</param>
        public TextToSpeechProperties(ITextToSpeechContent audioData = null)
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
        public static string ToFileName(TextToSpeechProperties properties)
        {
            // If key is empty or localization isn't available, use the simpler format
            if (string.IsNullOrEmpty(properties.Key) || !LocalizationSettings.HasSettings)
            {
                return $"TTS_{(properties.Speaker != "" ? $"{properties.Speaker}_" : "")}{properties.Locale?.Identifier.Code ?? LanguageSettings.Instance.ActiveOrDefaultLocale.Identifier.Code}_" +
                       $"{GetMd5Hash(properties.Text).Replace("-", "")}." +
                       $"{TextToSpeechSettings.GetFileTypeName(TextToSpeechSettings.Instance?.SelectedAudioType ?? TextToSpeechSettings.SupportedAudioType.WAV)}";
            }
            
            // Otherwise use the full format with table and key
            // Speaker_LocalisationTable_Key_Locale_TextHash.Type
            return $"TTS_{(properties.Speaker != "" ? $"{properties.Speaker}_" : "")}" +
                   $"{(string.IsNullOrEmpty(properties.Table)? RuntimeConfigurator.Instance.GetProcessStringLocalizationTable(): properties.Table)}_" +
                   $"{properties.Key}_" +
                   $"{(!properties.Locale? LanguageSettings.Instance.ActiveOrDefaultLocale.Identifier.Code : properties.Locale.Identifier.Code)}_" +
                   $"{GetMd5Hash(properties.Text).Replace("-", "")}." +
                   $"{TextToSpeechSettings.GetFileTypeName(TextToSpeechSettings.Instance?.SelectedAudioType ?? TextToSpeechSettings.SupportedAudioType.WAV)}";
        }
	}
}