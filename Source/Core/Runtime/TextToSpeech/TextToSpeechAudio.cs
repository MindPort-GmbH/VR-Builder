// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Source.TextToSpeech;
using VRBuilder.Core.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Primitives;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Runtime.Utils;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.TextToSpeech.Utils;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.TextToSpeech
{
	/// <summary>
	/// This class retrieves and stores AudioClips generated based in a provided localized text.
	/// </summary>
	[DataContract(IsReference = true)]
	[Attributes.DisplayName("Play Text to Speech")]
	public class TextToSpeechAudio : TextToSpeechContent, IAudioData, ITextToSpeechAudio
	{
		private bool isReady;
		private bool isLoading;
		private string text;
		private string speaker;
		private IAudioClip audioClip;

		/// <inheritdoc/>
		[DataMember]
		[UsesSpecificProcessDrawer("MultiLineStringDrawer")]
		[Attributes.DisplayName("Text/Key")]
		public override string Text
		{
			get => text;
			set => text = value;
		}

		/// <inheritdoc/>
		[DataMember]
		[UsesSpecificProcessDrawer("SpeakerDropdownDrawer")]
		[Attributes.DisplayName("Selected Profile")]
		public override string Speaker
		{
			get => speaker;
			set => speaker = value;
		}

		protected TextToSpeechAudio() : this("")
		{
		}

		public TextToSpeechAudio(string text, string speaker = "")
		{
			this.text = text;
			this.speaker = speaker;

			if (LocalizationSettings.HasSettings)
			{
				LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
			}
		}

		/// <summary>
		/// True when there is an Audio Clip loaded.
		/// </summary>
		public bool HasAudio
		{
			get
			{
				if (IsEmpty())
					return false;
				return audioClip != null;
			}
		}

		/// <inheritdoc/>
		bool IAudioData.IsReady => isReady;

		/// <inheritdoc/>
		bool IAudioData.IsLoading => isLoading;

		/// <inheritdoc/>
		public IAudioClip AudioClip
		{
			get => audioClip;
			private set => audioClip = value;
		}

		/// <inheritdoc/>
		public string ClipData
		{
			get => Text;
			set => Text = value;
		}

		/// <summary>
		/// Creates the audio clip based on the provided parameters.
		/// </summary>
		public async void Initialize()
		{
#if UNITY_EDITOR
			// Refresh the clip if the clip name changed
			if (isReady && AudioClip?.Name != text)
			{
				AudioClip = null;
			}
#endif
			if (isReady && AudioClip != null)
			{
				return;
			}

			AudioClip = null;
			isReady = false;
			isLoading = true;

			if (IsEmpty())
			{
				ForwardingLogger.LogWarning($"No text provided.");
				isLoading = false;
				return;
			}

			ITextToSpeechProvider provider = new FileTextToSpeechProvider();
			TextToSpeechFileLocator textToSpeechFileLocator = new TextToSpeechFileLocator();
			string table = ServiceRegistry.Get<ILanguageService>().ProcessStringLocalizationTable;

			string usedKey = "";

			if (table != "")
			{
				string usedText = GetLocalizedContent();

				// Text is the used key then instead of the text thas needs to be spoken
				textToSpeechFileLocator.WithKey(text).WithText(usedText).WithTable(table);
			}
			else
			{
				textToSpeechFileLocator.WithKey(text);
			}

			// Try set the current locale
			if (LocalizationSettings.HasSettings)
			{
				textToSpeechFileLocator.Locale = LocalizationSettings.SelectedLocale.ToCultureInfo();
			}

			textToSpeechFileLocator.WithSpeaker(speaker);

			// Synchronize the clip loading because of the async nature of I/O audio loading
			try
			{
				Task<IAudioClip> task = provider.ConvertTextToSpeech(textToSpeechFileLocator);

				// Capture the main thread context and returns to it
				AudioClip = await task;

				isReady = AudioClip != null;
			}
			catch (Exception e)
			{
				ForwardingLogger.LogException(e);
				isReady = false;
			}
			finally
			{
				isLoading = false;
			}
		}

		private void OnSelectedLocaleChanged(Locale locale)
		{
			if (Application.isPlaying && !IsEmpty())
			{
				Initialize();
			}
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return Text == null || string.IsNullOrEmpty(Text);
		}

		public override string GetLocalizedContent()
		{
			return ServiceRegistry.Get<ILanguageService>().GetLocalizedString(Text);
		}
	}
}
