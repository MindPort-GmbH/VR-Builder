using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// This behavior sets the next chapter to an arbitrary chapter and fast forwards to the end of the current chapter.
    /// </summary>
    [DataContract(IsReference = true)]
    public class GoToChapterBehavior : Behavior<GoToChapterBehavior.EntityData>
    {
        /// <summary>
        /// Behavior data.
        /// </summary>
        [DisplayName("Start Chapter")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            [DataMember]
            public Guid ChapterGuid { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        [JsonConstructor, Preserve]
        public GoToChapterBehavior() : this(Guid.Empty)
        {
        }

        public GoToChapterBehavior(Guid chapterGuid, string name = "Go to Chapter")
        {
            Data.ChapterGuid = chapterGuid;
            Data.Name = name;
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                if (Data.ChapterGuid == null || Data.ChapterGuid == Guid.Empty)
                {
                    return;
                }

                IChapter chapter = ProcessRunner.Current.Data.Chapters.FirstOrDefault(chapter => chapter.ChapterMetadata.Guid == Data.ChapterGuid);

                if (chapter != null)
                {
                    ProcessRunner.SetNextChapter(chapter);
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
            }

            /// <inheritdoc />
            public override void FastForward()
            {
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
