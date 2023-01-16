using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.EntityOwners;
using VRBuilder.TextToSpeech;
using VRBuilder.TextToSpeech.Audio;
using System.Linq;

namespace VRBuilder.Editor.TextToSpeech
{
    public static class TextToSpeechEditorUtils
    {
        public static async Task CacheAudioClip(string text, TextToSpeechConfiguration configuration)
        {
            string filename = configuration.GetUniqueTextToSpeechFilename(text);
            string filePath = $"{configuration.StreamingAssetCacheDirectoryName}/{filename}";

            ITextToSpeechProvider provider = TextToSpeechProviderFactory.Instance.CreateProvider(configuration);
            AudioClip audioClip = await provider.ConvertTextToSpeech(text);

            CacheAudio(audioClip, filePath, new NAudioConverter()); // TODO expose converter to configuration
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

        public static async void CacheAllClips()
        {
            TextToSpeechConfiguration configuration = RuntimeConfigurator.Configuration.GetTextToSpeechConfiguration();

            foreach (TextToSpeechAudio clip in configuration.RegisteredClips) 
            {
                if (string.IsNullOrEmpty(clip.Text) == false)
                {
                    await CacheAudioClip(clip.Text, configuration);
                }
            }
        }

        public static IEnumerable<TextToSpeechAudio> GetTTSFromProcess(IProcess process)
        {
            List<TextToSpeechAudio> tts = new List<TextToSpeechAudio>();

            foreach (IChapter chapter in process.Data.GetChildren())
            {
                tts.AddRange(GetTTSFromChapter(chapter));
            }

            return tts;
        }

        public static IEnumerable<TextToSpeechAudio> GetTTSFromChapter(IChapter chapter)
        {
            List<TextToSpeechAudio> tts = new List<TextToSpeechAudio>();

            foreach (IStep step in chapter.Data.GetChildren())
            {
                tts.AddRange(GetTTSFromStep(step));
            }

            return tts;
        }

        public static IEnumerable<TextToSpeechAudio> GetTTSFromStep(IStep step)
        {
            List<TextToSpeechAudio> tts = new List<TextToSpeechAudio>();

            IEnumerable<IBehaviorCollection> behaviorCollections = step.Data.GetChildren().Where(child => child is BehaviorCollection).Cast<IBehaviorCollection>();

            foreach (IBehaviorCollection behaviorCollection in behaviorCollections)
            {
                foreach (IBehavior behavior in behaviorCollection.Data.Behaviors)
                {
                    tts.AddRange(GetTTSFromBehavior(behavior));

                    if (behavior.Data is IEntityCollectionData<IBehavior>)
                    {
                        IEntityCollectionData<IBehavior> data = behavior.Data as IEntityCollectionData<IBehavior>;

                        foreach (IBehavior childBehavior in data.GetChildren())
                        {
                            tts.AddRange(GetTTSFromBehavior(childBehavior));
                        }
                    }

                    if (behavior.Data is IEntityCollectionData<IChapter>)
                    {
                        IEntityCollectionData<IChapter> data = behavior.Data as IEntityCollectionData<IChapter>;

                        foreach (IChapter childChapter in data.GetChildren())
                        {
                            tts.AddRange(GetTTSFromChapter(childChapter));
                        }
                    }
                }

            }

            return tts;
        }

        public static IEnumerable<TextToSpeechAudio> GetTTSFromBehavior(IBehavior behavior)
        {
            List<TextToSpeechAudio> tts = new List<TextToSpeechAudio>();

            IEnumerable<PropertyInfo> properties = behavior.Data.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                IEnumerable<ParameterInfo> indexes = property.GetIndexParameters();

                if (indexes.Count() > 0)
                {
                    foreach (ParameterInfo index in indexes)
                    {
                        object value = property.GetValue(behavior.Data, new object[] { index });

                        if (value != null && (value.GetType() == typeof(TextToSpeechAudio) || value.GetType().IsSubclassOf(typeof(TextToSpeechAudio))))
                        {
                            tts.Add(value as TextToSpeechAudio);
                        }
                    }
                }
                else
                {
                    object value = property.GetValue(behavior.Data);

                    if (value != null && (value.GetType() == typeof(TextToSpeechAudio) || value.GetType().IsSubclassOf(typeof(TextToSpeechAudio))))
                    {
                        tts.Add(value as TextToSpeechAudio);
                    }
                }
            }

            return tts;
        }
    }
}