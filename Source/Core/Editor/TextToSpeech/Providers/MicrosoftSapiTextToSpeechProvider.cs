using SpeechLib;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.TextToSpeech.Utils;

namespace VRBuilder.Core.Editor.TextToSpeech.Providers
{
    /// <summary>
    /// TTS provider which uses Microsoft SAPI to generate audio.
    /// TextToSpeechConfig.Voice has to be either "male", "female", or "neutral".
    /// TextToSpeechConfig.Language is a language code ("de" or "de-DE" for German, "en" or "en-US" for English).
    /// It runs the TTS synthesis in a separate thread, saving the result to a temporary cache file.
    /// </summary>
    public class MicrosoftSapiTextToSpeechProvider : ITextToSpeechProvider
    {
        private MicrosoftTextToSpeechConfiguration configuration;

        /// <summary>
        /// This is the template of the Speech Synthesis Markup Language (SSML) string used to change the language and voice.
        /// The first argument is the preferred language code (Examples: "de" or "de-DE" for German, "en" or "en-US" for English). If the language is not installed on the system, it chooses English.
        /// The second argument is the preferred gender of the voice ("male", "female", or "neutral"). If it is not installed, it chooses another gender.
        /// The third argument is a string which is read out loud.
        /// </summary>
        private const string ssmlTemplate = "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{0}'><voice languages='{0}' gender='{1}' required='languages' optional='gender'>{2}</voice></speak>";

        /// <summary>
        /// Remove the file at path and remove empty folders.
        /// </summary>
        private static void ClearCache(string path)
        {
            File.Delete(path);

            while (string.IsNullOrEmpty(path) == false)
            {
                path = Directory.GetParent(path).ToString();

                if (Directory.Exists(path) && Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0)
                {
                    Directory.Delete(path);
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Set up a file stream by path.
        /// </summary>
        private static SpFileStream PrepareFileStreamToWrite(string path)
        {
            SpFileStream stream = new SpFileStream();
            SpAudioFormat format = new SpAudioFormat();
            format.Type = SpeechAudioFormatType.SAFT48kHz16BitMono;
            stream.Format = format;
            stream.Open(path, SpeechStreamFileMode.SSFMCreateForWrite, true);

            return stream;
        }

        /// <inheritdoc />
        public void SetConfig(ITextToSpeechConfiguration configuration)
        {
            this.configuration = configuration as MicrosoftTextToSpeechConfiguration;
        }

        /// <inheritdoc />
        public ITextToSpeechConfiguration LoadConfig()
        {
            return MicrosoftTextToSpeechConfiguration.Instance;
        }

        /// <inheritdoc />
        public bool SupportsMultiSpeaker()
        {
            return false;
        }

        /// <inheritdoc />
        public Task<AudioClip> ConvertTextToSpeech(string key, string text, Locale locale)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

            if(configuration == null)
                configuration = MicrosoftTextToSpeechConfiguration.Instance;
            
            // Check the validity of the voice in the configuration.
            // If it is invalid, change it to neutral.
            string voice = configuration.Voice;
            switch (voice.ToLower())
            {
                case "female":
                    voice = "female";
                    break;
                case "male":
                    voice = "male";
                    break;
                default:
                    voice = "neutral";
                    break;
            }

            string filePath = configuration.PrepareFilepathForText(key, text, locale);
            float[] sampleData = Synthesize(text, filePath, locale.Identifier.Code, voice);

            AudioClip audioClip = AudioClip.Create(text, channels: 1, frequency: 48000, lengthSamples: sampleData.Length, stream: false);
            audioClip.SetData(sampleData, 0);

            return Task.FromResult(audioClip);
#else
            throw new PlatformNotSupportedException($"TTS audio '{text}' could not be generated due that {GetType().Name} is not supported in {Application.platform}");
#endif
        }

        private float[] Synthesize(string text, string outputPath, string language, string voice)
        {
            // Despite the fact that SpVoice.AudioOutputStream accepts values of type ISpeechBaseStream,
            // the single type of a stream that is actually working is a SpFileStream.
            SpFileStream stream = PrepareFileStreamToWrite(outputPath);
            SpVoice synthesizer = new SpVoice { AudioOutputStream = stream };

            string ssmlText = string.Format(ssmlTemplate, language, voice, text);
            synthesizer.Speak(ssmlText, SpeechVoiceSpeakFlags.SVSFIsXML);
            synthesizer.WaitUntilDone(-1);
            stream.Close();

            byte[] data = File.ReadAllBytes(outputPath);
            float[] sampleData = TextToSpeechUtils.ShortsInByteArrayToFloats(data);
            float[] cleanData = TextToSpeechUtils.RemoveArtifacts(sampleData);

            ClearCache(outputPath);

            return cleanData;
        }
    }
}
