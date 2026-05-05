using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.TextToSpeech;
using static VRBuilder.Core.TextToSpeech.Utils.TextToSpeechUtils;

namespace Source.Core.Runtime.TextToSpeech.Utils
{
	/// <summary>
	/// Helper class for creating unique file names for text-to-speech audio files.
	/// Only non-null/non-empty fields are included in the generated filename.
	/// </summary>
	public class TextToSpeechFileProperties
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
        public TextToSpeechFileProperties WithKey(string key)
        {
            Key = key;
            return this;
        }

        /// <summary>
        /// Sets the text property and returns the instance for chaining.
        /// </summary>
        public TextToSpeechFileProperties WithText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets the locale property and returns the instance for chaining.
        /// </summary>
        public TextToSpeechFileProperties WithLocale(Locale locale)
        {
            Locale = locale;
            return this;
        }

        /// <summary>
        /// Sets the table property and returns the instance for chaining.
        /// </summary>
        public TextToSpeechFileProperties WithTable(string table)
        {
            Table = table;
            return this;
        }

        /// <summary>
        /// Sets the speaker property and returns the instance for chaining.
        /// </summary>
        public TextToSpeechFileProperties WithSpeaker(string speaker)
        {
            Speaker = speaker;
            return this;
        }

        /// <summary>
        /// Sets the format property and returns the instance for chaining.
        /// </summary>
        public TextToSpeechFileProperties WithFormat(TextToSpeechSettings.SupportedAudioType format)
        {
            Format = format;
            return this;
        }

        /// <summary>
        /// Create a new property to generate the unique file name of the text-to-speech file
        /// </summary>
        /// <param name="audioData">Used audio information for basic unique filename</param>
        public TextToSpeechFileProperties(ITextToSpeechContent audioData = null)
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
        public static string ToFileName(TextToSpeechFileProperties fileProperties)
        {
            // If key is empty or localization isn't available, use the simpler format
            if (string.IsNullOrEmpty(fileProperties.Key) || !LocalizationSettings.HasSettings)
            {
                return $"TTS_{(fileProperties.Speaker != "" ? $"{fileProperties.Speaker}_" : "")}{fileProperties.Locale?.Identifier.Code ?? ""}_" +
                       $"{GetMd5Hash(fileProperties.Text).Replace("-", "")}." +
                       $"{TextToSpeechSettings.GetFileTypeName(TextToSpeechSettings.Instance?.SelectedAudioType ?? TextToSpeechSettings.SupportedAudioType.WAV)}";
            }
            
            // Otherwise use the full format with table and key
            return $"TTS_{(fileProperties.Speaker != "" ? $"{fileProperties.Speaker}_" : "")}" +
                   $"{(string.IsNullOrEmpty(fileProperties.Table)? RuntimeConfigurator.Instance.GetProcessStringLocalizationTable(): fileProperties.Table)}_" +
                   $"{fileProperties.Key}_" +
                   $"{(!fileProperties.Locale? "en" : fileProperties.Locale.Identifier.Code)}_" +
                   $"{GetMd5Hash(fileProperties.Text).Replace("-", "")}." +
                   $"{TextToSpeechSettings.GetFileTypeName(TextToSpeechSettings.Instance?.SelectedAudioType ?? TextToSpeechSettings.SupportedAudioType.WAV)}";
        }
	}
}