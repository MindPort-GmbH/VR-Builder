using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Editor.ProcessAssets;
using VRBuilder.Core.Localization;
using VRBuilder.Core.Settings;
using VRBuilder.Core.TextToSpeech;
using VRBuilder.Core.TextToSpeech.Configuration;
using VRBuilder.Core.TextToSpeech.Providers;
using VRBuilder.Core.TextToSpeech.Utils;
using VRBuilder.TextToSpeech;
using static VRBuilder.Core.Editor.ProcessAssets.ProcessAssetUtils;
using Application = UnityEngine.Application;
using Task = System.Threading.Tasks.Task;

namespace VRBuilder.Core.Editor.TextToSpeech.Utils
{
    public static class TextToSpeechEditorUtils
    {
        private static string lastStreamingAssetCacheDirectoryName;
        private static string[] assetPaths;
        
        /// <summary>
        /// Generates TTS audio and creates a file.
        /// </summary>
        public static async Task CacheAudioClip(string key, string text, Locale locale)
        {
            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider();
            ITextToSpeechConfiguration configuration = provider.LoadConfig();
            string filename = configuration.GetUniqueTextToSpeechFilename(key, text, locale);
            string filePath = $"{RuntimeConfigurator.Configuration.GetTextToSpeechSettings().StreamingAssetCacheDirectoryName}/{filename}";
            AudioClip audioClip = await provider.ConvertTextToSpeech(key, text, locale);

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

            if (Provider.enabled && Provider.onlineState != OnlineState.Offline)
            {
                Provider.Checkout(absoluteFilePath, CheckoutMode.Both);
            }
            return converter.TryWriteAudioClipToFile(audioClip, absoluteFilePath);
        }

        /// <summary>
        /// Generates files for all <see cref="TextToSpeechAudio"/> passed.
        /// </summary>
        public static async Task<int> CacheTextToSpeechClips(IEnumerable<ITextToSpeechContent> clips, Locale locale, string localizationTable)
        {
            TextToSpeechSettings settings = RuntimeConfigurator.Configuration.GetTextToSpeechSettings();
            ITextToSpeechContent[] validClips = clips.Where(clip => string.IsNullOrEmpty(clip.Text) == false).ToArray();
            for (int i = 0; i < validClips.Length; i++)
            {
                string key = null;
                string text = validClips[i].Text;
                if (string.IsNullOrEmpty(localizationTable) == false)
                {
                    text = LanguageUtils.GetLocalizedString(validClips[i].Text, localizationTable, locale);
                }

                if (text != validClips[i].Text)
                {
                    key = validClips[i].Text;
                }

                if (EditorUtility.DisplayCancelableProgressBar($"Generating audio with {settings.Provider} in locale {locale.Identifier.Code}", $"{i + 1}/{validClips.Length}: {text}", (float) i / validClips.Length))
                {
                    break;
                }

                try
                {
                    await CacheAudioClip(key, text, locale);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Failed to create clip '{text}' for locale {locale.Identifier}\n{e}");
                }
            }

            RevertUnchangedAssets();

            EditorUtility.ClearProgressBar();
            return validClips.Length;
        }

        /// <summary>
        /// Revert unchanged assets for active version control, so not every file is listed as new
        /// </summary>
        private static void RevertUnchangedAssets()
        {
            if (Provider.enabled && Provider.onlineState != OnlineState.Offline)
            {
                string[] assetPaths = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/StreamingAssets/" + RuntimeConfigurator.Configuration.GetTextToSpeechSettings().StreamingAssetCacheDirectoryName });
				if (assetPaths.Length > 0)
				{
					AssetList assetList = assetPaths.Aggregate(new AssetList(), (list, asset) =>
					{
						list.Add(new Asset(asset));
						return list;
					});
					Provider.Revert(assetList, RevertMode.Unchanged);
                }
            }
        }

        /// <summary>
        /// Texts of a specific TTS Content is cached
        /// </summary>
        /// <param name="configuration">Text to speech configuration that is used for this caching</param>
        /// <param name="content">Text for cache checking</param>
        /// <returns>True if the text is already cached as a TTS file</returns>
        public static bool IsCached(this ITextToSpeechConfiguration configuration, ITextToSpeechContent content)
        {
            var currentDirName = RuntimeConfigurator.Configuration.GetTextToSpeechSettings().StreamingAssetCacheDirectoryName;
            
            if (currentDirName != lastStreamingAssetCacheDirectoryName)
            {
                lastStreamingAssetCacheDirectoryName = currentDirName;

                assetPaths = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/StreamingAssets/" + currentDirName });
                if (assetPaths.Length == 0)
				{
					return false;
                }
            }

            string key = content.Text; //key can be refactored out, when we have MD5 of both original text and translated text.
            string md5 = TextToSpeechUtils.GetMd5Hash(content.Text); //TODO: this will not detect translated text... maybe we should hash the text everywhere before translating

            return assetPaths.Any(assetPath => assetPath.Contains(md5) || assetPath.Contains(key));
        }
        
        /// <summary>
        /// Generates text-to-speech audio for the selected process, locale and configuration
        /// </summary>
        /// <param name="processName">The selected process where the audio should be generated</param>
        /// <param name="locale">The selected language in which the audio should be generated</param>
        /// <param name="configuration">The selected text-to-speech configuration</param>
        public static async Task<bool> GenerateTextToSpeechForProcess(string processName, Locale locale, ITextToSpeechConfiguration configuration)
        {
            IProcess process = ProcessAssetManager.Load(processName);
            if (process != null)
            {
                IEnumerable<ITextToSpeechContent> tts = EditorReflectionUtils
                    .GetNestedPropertiesFromData<ITextToSpeechContent>(process.Data)
                    .Where(content => !string.IsNullOrEmpty(content.Text) && !configuration.IsCached(content))
                    .ToList(); //this to list is necessary because if multi iteration
                if (tts.Any())
                {
                    if (process.ProcessMetadata == null || string.IsNullOrEmpty(process.ProcessMetadata.StringLocalizationTable) && LocalizationSettings.HasSettings)
                    {
                        UnityEngine.Debug.LogWarning($"... Unable to generate localized audio for process '{process.Data.Name}'. Localization appears to be in use but no string localization table has been selected in the process configuration.");
                        return false;
                    }

                    int clips = await CacheTextToSpeechClips(tts, locale, process.ProcessMetadata.StringLocalizationTable);
                    UnityEngine.Debug.Log($"... Generated {clips} audio files for process '{process.Data.Name}' with locale {locale}");
                    return true;
                }
                UnityEngine.Debug.Log($"... Did not find TextToSpeech Behaviors in this Process. Skipping!");
            }

            return false;
        }
        
        /// <summary>
        /// Generates text-to-speech audio for the selected process in all availed languages
        /// </summary>
        /// <param name="processName">The selected process where the audio should be generated</param>
        public static async Task GenerateTextToSpeechForProcess(string processName)
        {
            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider();
            ITextToSpeechConfiguration configuration = provider.LoadConfig();
            
            List<Locale> locales = BuildLocales().ToList();
            bool filesGenerated = false;
            
            UnityEngine.Debug.Log($"Generating TTS audio for all availed locales for the process {processName}");
            foreach (Locale locale in locales)
            {
                if (await GenerateTextToSpeechForProcess(processName, locale, configuration))
                {
                    filesGenerated = true;
                }
            }
            
            if (!filesGenerated)
            {
                UnityEngine.Debug.Log($"Found no TTS content to generate for all availed locales.");
            }
        }

        #region ALL PROCESSES
        
        /// <summary>
        /// Generates text-to-speech audio files for all available processes for the specified <paramref name="locale"/>.
        /// </summary>
        /// <param name="locale">The locale for which audio files should be generated.</param>
        public static async Task GenerateTextToSpeechForAllProcesses(Locale locale)
        {
            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider();
            ITextToSpeechConfiguration configuration = provider.LoadConfig();
            
            IEnumerable<string> processNames = GetAllProcesses();
            bool filesGenerated = false;

            UnityEngine.Debug.Log($"Generating TTS audio for locale {locale} for processes:");
            foreach (string processName in processNames)
            {
                UnityEngine.Debug.Log($"Generating TTS audio for process '{processName}' with locale {locale}...");
                if (await GenerateTextToSpeechForProcess(processName, locale, configuration))
                {
                    filesGenerated = true;
                }
            }

            if (!filesGenerated)
            {
                UnityEngine.Debug.Log($"Found no TTS content to generate for locale {locale}.");
            }
        }
        
        /// <summary>
        /// Generates TTS audio files for all project locales for all processes.
        /// </summary>
        public static async Task GenerateTextToSpeechForAllProcesses()
        {
            List<Locale> locales = BuildLocales().ToList();

            foreach (Locale locale in locales)
            {
                await GenerateTextToSpeechForAllProcesses(locale);
            }
        }
        
        /// <summary>
        /// Generates text-to-speech audio files for the active or default locale for all processes.
        /// </summary>
        public static async Task GenerateTextToSpeechForAllProcessesAndActiveOrDefaultLocale()
        {
            await GenerateTextToSpeechForAllProcesses(LanguageSettings.Instance.ActiveOrDefaultLocale);
            AssetDatabase.Refresh();
        }
        
        #endregion

        #region ACTIVE SCENE PROCESSES

        /// <summary>
        /// Generates the text-to-speech audio for the current scene in the current language
        /// </summary>
        public static async Task GenerateTextToSpeechActiveSceneAndActiveOrDefaultLocale()
        {
            await GenerateTextToSpeechActiveScene(LanguageSettings.Instance.ActiveOrDefaultLocale);
        }

        /// <summary>
        /// Generates the text-to-speech audio for the current scene with all availed languages
        /// </summary>
        public static async Task GenerateTextToSpeechActiveScene()
        {
            List<Locale> locales = BuildLocales().ToList();

            foreach (Locale locale in locales)
            {
                await GenerateTextToSpeechActiveScene(locale);
            }
        }
        
        /// <summary>
        /// Generates the text-to-speech audio for the current scene in the selected language
        /// </summary>
        public static async Task GenerateTextToSpeechActiveScene(Locale locale)
        {
            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider();
            ITextToSpeechConfiguration configuration = provider.LoadConfig();
            var activeScene = SceneManager.GetActiveScene();
            
            var runtimeConfigurator = activeScene.GetRootGameObjects().Select(o => o.GetComponentInChildren<RuntimeConfigurator>(true)).First(o => o != null);
            if (runtimeConfigurator != null)
            {
                await GenerateTextToSpeechForProcess(GetProcessNameFromPath(runtimeConfigurator.GetSelectedProcess()), locale, configuration);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"No active process config found. TTS files for current local {locale} in the active scene {activeScene.name} could not be generated.");
            }
            
            AssetDatabase.Refresh();
        }
        
        #endregion
        
        /// <summary>
        /// Get an enumerable list of the locales for building processes
        /// </summary>
        /// <returns>A yielded local as a list or the active/default one</returns>
        private static IEnumerable<Locale> BuildLocales()
        {
            if (LocalizationSettings.HasSettings)
                foreach (Locale availableLocalesLocale in LocalizationSettings.AvailableLocales.Locales)
                    yield return availableLocalesLocale;
            else
                yield return LanguageSettings.Instance.ActiveOrDefaultLocale;
        }
    }
}