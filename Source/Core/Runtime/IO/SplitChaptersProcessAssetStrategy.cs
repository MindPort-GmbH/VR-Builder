using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    /// <summary>
    /// Asset strategy that saves the process as a list of chapter, then each chapter in a separate file.
    /// A manifest file is created as well.
    /// </summary>
    public class SplitChaptersProcessAssetStrategy : IProcessAssetStrategy
    {
        /// <inheritdoc/>
        public bool CreateManifest => true;

        /// <inheritdoc/>
        public IDictionary<string, byte[]> CreateSerializedProcessAssets(IProcess process, IProcessSerializer serializer)
        {
            Dictionary<string, byte[]> serializedAssets = new Dictionary<string, byte[]>();

            List<IChapter> chapterRefs = new List<IChapter>(process.Data.Chapters.Select(chapter => new ChapterRef(chapter)));
            List<IChapter> originalChapters = new List<IChapter>(process.Data.Chapters);

            try
            {
                process.Data.Chapters = chapterRefs;
                serializedAssets.Add(process.Data.Name, serializer.ProcessToByteArray(process));
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            // Restore the process to the original state.
            process.Data.Chapters = originalChapters;

            foreach(IChapter chapter in process.Data.Chapters)
            {
                serializedAssets.Add($"Chapter-{chapter.ChapterMetadata.Guid}", serializer.ChapterToByteArray(chapter));
            }

            return serializedAssets;
        }

        /// <inheritdoc/>
        public IProcess GetProcessFromSerializedData(byte[] processData, IEnumerable<byte[]> additionalData, IProcessSerializer serializer)
        {
            IProcess process = serializer.ProcessFromByteArray(processData);
            IEnumerable<IChapter> chapters = additionalData.Select(data => serializer.ChapterFromByteArray(data));

            List<IChapter> deserializedChapters = new List<IChapter>();

            foreach(IChapter chapter in process.Data.Chapters)
            {
                IChapter deserializedChapter = chapters.FirstOrDefault(c => c.ChapterMetadata.Guid == chapter.ChapterMetadata.Guid);

                if(deserializedChapter == null)
                {
                    Debug.LogError($"Error loading the process. Could not find chapter with id: {chapter.ChapterMetadata.Guid}");
                }

                deserializedChapters.Add(deserializedChapter);
            }

            process.Data.Chapters = deserializedChapters;

            return process;
        }

        /// <summary>
        /// Chapter dummy class that only stores the metadata of the specified chapter.
        /// </summary>
        [Serializable]
        private class ChapterRef : Entity<Chapter.EntityData>, IChapter
        {
            [JsonConstructor]
            public ChapterRef()
            {
            }

            public ChapterRef(IChapter chapter)
            {
                Data.Steps = new List<IStep>();
                ChapterMetadata = chapter.ChapterMetadata;
            }

            [DataMember]
            public ChapterMetadata ChapterMetadata { get; set; }

            IChapterData IDataOwner<IChapterData>.Data => Data;

            public IChapter Clone()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
