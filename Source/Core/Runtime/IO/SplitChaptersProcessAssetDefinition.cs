using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using VRBuilder.Core.Serialization;

namespace VRBuilder.Core.IO
{
    public class SplitChaptersProcessAssetDefinition : IProcessAssetDefinition
    {
        public IDictionary<string, byte[]> CreateSerializedProcessAssets(IProcess process, IProcessSerializer serializer)
        {
            Dictionary<string, byte[]> serializedAssets = new Dictionary<string, byte[]>();

            List<IChapter> chapterRefs = new List<IChapter>(process.Data.Chapters.Select(chapter => new ChapterRef(chapter)));
            List<IChapter> originalChapters = new List<IChapter>(process.Data.Chapters);

            process.Data.Chapters = chapterRefs;
            serializedAssets.Add(process.Data.Name, serializer.ProcessToByteArray(process));
            process.Data.Chapters = originalChapters;

            foreach(IChapter chapter in process.Data.Chapters)
            {
                serializedAssets.Add($"chapter-{chapter.ChapterMetadata.Guid}", serializer.ChapterToByteArray(chapter));
            }

            return serializedAssets;
        }

        public IProcess GetProcessFromSerializedData(byte[] processData, IEnumerable<byte[]> additionalData, IProcessSerializer serializer)
        {
            IProcess process = serializer.ProcessFromByteArray(processData);
            IEnumerable<IChapter> chapters = additionalData.Select(data => serializer.ChapterFromByteArray(data));

            List<IChapter> chapterList = new List<IChapter>();

            foreach(IChapter chapter in process.Data.Chapters)
            {
                IChapter deserializedChapter = chapters.FirstOrDefault(c => c.ChapterMetadata.Guid == chapter.ChapterMetadata.Guid);
                if(deserializedChapter == null)
                {
                    Debug.LogError("Chapter not found.");
                }
                chapterList.Add(deserializedChapter);
            }

            process.Data.Chapters = chapterList;

            return process;
        }

        [DataContract(IsReference = true)]
        private class ChapterRef : Entity<Chapter.EntityData>, IChapter
        {
            [JsonConstructor]
            public ChapterRef() : this(null) { }

            public ChapterRef(IChapter chapter)
            {
                Data.Steps = new List<IStep>();

                if(chapter != null)
                {
                    ChapterMetadata = chapter.ChapterMetadata;
                }
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
