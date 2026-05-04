using UnityEngine.Localization;
using VRBuilder.Core.TextToSpeech;

namespace Source.Core.Runtime.TextToSpeech.Utils
{
	/// <summary>
	/// Helper class for creating unique file names for text-to-speech audio files.
	/// Only non-null/non-empty fields are included in the generated filename.
	/// </summary>
	public class TextToSpeechFileNameProperties
	{
		/// <summary>
		/// Used key of the text-to-speech audio data.
		/// </summary>
		public string key = "";
            
		/// <summary>
		/// Text content of the text-to-speech audio.
		/// </summary>
		public string text = "";
            
		/// <summary>
		/// Language of the text-to-speech audio.
		/// </summary>
		public Locale locale = null;
            
		/// <summary>
		/// Used localization table name of the text-to-speech audio data.
		/// </summary>
		public string table = "";
            
		/// <summary>
		/// Used speaker of the text-to-speech audio.
		/// </summary>
		public string speaker = "";
            
		/// <summary>
		/// Used format of the text-to-speech audio.
		/// </summary>
		public TextToSpeechSettings.SupportedAudioType format = TextToSpeechSettings.SupportedAudioType.WAV;
            
		/// <summary>
		/// Creates a new instance with the provided properties.
		/// </summary>
		public TextToSpeechFileNameProperties(string key = "", string text = "", Locale locale = null, string table = "", string speaker = "", TextToSpeechSettings.SupportedAudioType format = TextToSpeechSettings.SupportedAudioType.WAV)
		{
			this.key = key;
			this.text = text;
			this.locale = locale;
			this.table = table;
			this.speaker = speaker;
			this.format = format;
		}
	}
}