using VRBuilder.Core.Configuration;

namespace VRBuilder.Core.TextToSpeech.Configuration
{
    /// <summary>
    /// TextToSpeech extensions methods for <see cref="BaseRuntimeConfiguration"/>.
    /// </summary>
    public static class RuntimeConfigurationExtensions
    {
        public static TextToSpeechSettings GetTextToSpeechSettings(this BaseRuntimeConfiguration runtimeConfiguration)
        {
            return TextToSpeechSettings.Instance;
        }
    }
}
