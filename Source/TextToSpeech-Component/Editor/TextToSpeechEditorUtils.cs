using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Localization;
using VRBuilder.TextToSpeech;
using VRBuilder.TextToSpeech.Audio;

namespace VRBuilder.Editor.TextToSpeech
{
    public static class TextToSpeechEditorUtils
    {
        /// <summary>
        /// Generates TTS audio and creates a file.
        /// </summary>
        public static async Task CacheAudioClip(string text, Locale locale, TextToSpeechConfiguration configuration)
        {
            string filename = configuration.GetUniqueTextToSpeechFilename(text, locale);
            string filePath = $"{configuration.StreamingAssetCacheDirectoryName}/{filename}";

            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider(configuration);
            AudioClip audioClip = await provider.ConvertTextToSpeech(text, locale);

            CacheAudio(audioClip, filePath, new NAudioConverter());
        }

        /// <summary>
        /// Stores given <paramref name="audioClip"/> in a cached directory.
        /// </summary>
        /// <remarks>When used in the Unity Editor the cached directory is inside the StreamingAssets folder; Otherwise during runtime the base path is the platform
        /// persistent data.</remarks>
        /// <param name="audioClip">The audio file to be cached.</param>
        /// <param name="filePath">Relative path where the <paramref name="audioClip"/> will be stored.</param>
        /// <returns>True if the file was successfully cached.</returns>
        private static bool CacheAudio(AudioClip audioClip, string filePath, IAudioConverter converter)
        {
            // Ensure target directory exists.
            string fileName = Path.GetFileName(filePath);
            string relativePath = Path.GetDirectoryName(filePath);

            string basedDirectoryPath = Application.isEditor ? Application.streamingAssetsPath : Application.persistentDataPath;
            string absolutePath = Path.Combine(basedDirectoryPath, relativePath);

            if (string.IsNullOrEmpty(absolutePath) == false && Directory.Exists(absolutePath) == false)
            {
                Directory.CreateDirectory(absolutePath);
            }

            string absoluteFilePath = Path.Combine(absolutePath, fileName);

            return converter.TryWriteAudioClipToFile(audioClip, absoluteFilePath);
        }

        /// <summary>
        /// Generates files for all <see cref="TextToSpeechAudio"/> passed.
        /// </summary>
        public static async Task<int> CacheTextToSpeechClips(IEnumerable<ITextToSpeechContent> clips, Locale locale, string localizationTable)
        {
            TextToSpeechConfiguration configuration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();
            ITextToSpeechContent[] validClips = clips.Where(clip => string.IsNullOrEmpty(clip.Text) == false).ToArray();
            for (int i = 0; i < validClips.Length; i++)
            {
                string text = validClips[i].Text;
                if (string.IsNullOrEmpty(localizationTable) == false)
                {
                    text = LanguageUtils.GetLocalizedString(validClips[i].Text, localizationTable, locale);
                }
                EditorUtility.DisplayProgressBar($"Generating audio with {configuration.Provider} in locale {locale.Identifier.Code}", $"{i + 1}/{validClips.Length}: {text}", (float)i / validClips.Length);
                await CacheAudioClip(text, locale, configuration);
            }
            EditorUtility.ClearProgressBar();
            return validClips.Length;
        }

        /// <summary>
        /// Generates TTS audio files for all available processes for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which audio files should be generated.</param>
        public static async Task GenerateTextToSpeechForAllProcesses(Locale locale)
        {
            IEnumerable<string> processNames = ProcessAssetUtils.GetAllProcesses();
            bool filesGenerated = false;

            foreach (string processName in processNames)
            {
                IProcess process = ProcessAssetManager.Load(processName);
                if (process != null)
                {
                    IEnumerable<ITextToSpeechContent> tts = EditorReflectionUtils.GetNestedPropertiesFromData<ITextToSpeechContent>(process.Data).Where(content => content.IsCached(locale) == false && string.IsNullOrEmpty(content.Text) == false);
                    if (tts.Count() > 0)
                    {
                        if(string.IsNullOrEmpty(process.ProcessMetadata.StringLocalizationTable) && LocalizationSettings.HasSettings)
                        {
                            Debug.LogError($"Unable to generate localized audio for process '{process.Data.Name}'. Localization appears to be in use but no string localization table has been selected in the process configuration.");
                            continue;
                        }
                        filesGenerated = true;
                        int clips = await CacheTextToSpeechClips(tts, locale, process.ProcessMetadata.StringLocalizationTable);
                        Debug.Log($"Generated {clips} audio files for process '{process.Data.Name}' with locale {locale}");
                    }
                }
            }

            if (filesGenerated == false)
            {
                Debug.Log($"Found no TTS content to generate for locale {locale}.");
            }
        }

        /// <summary>
        /// Generates TTS audio files for the the active or default locale for all processes.
        /// </summary>
        public static async void GenerateTextToSpeechForAllProcessesAndActiveOrDefaultLocale()
        {
            await TextToSpeechEditorUtils.GenerateTextToSpeechForAllProcesses(LanguageSettings.Instance.ActiveOrDefaultLocale);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Generates TTS audio files for all project locales for all processes.
        /// </summary>
        public static async void GenerateTextToSpeechForAllProcesses()
        {
            List<Locale> locales = new List<Locale>();

            if (LocalizationSettings.HasSettings)
            {
                locales = LocalizationSettings.AvailableLocales.Locales;
            }
            else
            {
                locales.Add(LanguageSettings.Instance.ActiveOrDefaultLocale);
            }

            foreach (Locale locale in locales)
            {
                await GenerateTextToSpeechForAllProcesses(locale);
            }

            AssetDatabase.Refresh();
        }
    }
}