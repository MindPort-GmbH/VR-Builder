using VRBuilder.Core.Configuration;

namespace VRBuilder.TextToSpeech
{
    /// <summary>
    /// TextToSpeech extensions methods for <see cref="BaseRuntimeConfiguration"/>.
    /// </summary>
    public static class RuntimeConfigurationExtensions
    {
        /// <summary>
        /// Text to speech configuration.
        /// </summary>
        private static MicrosoftTextToSpeechConfiguration TextToSpeechConfiguration;

        /// <summary>
        /// Return loaded <see cref="MicrosoftTextToSpeechConfiguration"/>.
        /// </summary>
        public static MicrosoftTextToSpeechConfiguration GetTextToSpeechConfiguration(this BaseRuntimeConfiguration runtimeConfiguration)
        {
            if (TextToSpeechConfiguration == null)
            {
                TextToSpeechConfiguration = MicrosoftTextToSpeechConfiguration.LoadConfiguration();
            }

            return TextToSpeechConfiguration;
        }

        public static TextToSpeechSettings GetTextToSpeechSettings(this BaseRuntimeConfiguration runtimeConfiguration)
        {
            return TextToSpeechSettings.Instance;
        }

        /// <summary>
        /// Loads a new <see cref="MicrosoftTextToSpeechConfiguration"/>
        /// </summary>
        public static void SetTextToSpeechConfiguration(this BaseRuntimeConfiguration runtimeConfiguration,
            MicrosoftTextToSpeechConfiguration ttsConfiguration)
        {
            TextToSpeechConfiguration = ttsConfiguration;
        }
    }
}