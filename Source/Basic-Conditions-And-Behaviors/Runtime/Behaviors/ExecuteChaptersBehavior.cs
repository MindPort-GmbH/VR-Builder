using System.Collections;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using VRBuilder.Core.EntityOwners;
using System.Collections.Generic;
using System.Linq;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Behavior that executes a number of chapters at the same time and completes when the chapters ends.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ExecuteChaptersBehavior : Behavior<ExecuteChaptersBehavior.EntityData>
    {
        /// <summary>
        /// Execute chapter behavior data.
        /// </summary>
        [DisplayName("Execute Chapters")]
        [DataContract(IsReference = true)]
        public class EntityData : EntityCollectionData<IChapter>, IBehaviorData
        {
            [DataMember]
            public List<IChapter> Chapters { get; set; }

            public string Name { get; set; }

            public override IEnumerable<IChapter> GetChildren()
            {
                return Chapters;
            }
        }

        [JsonConstructor, Preserve]
        public ExecuteChaptersBehavior() : this(chapters: new List<IChapter>())
        {
        }

        public ExecuteChaptersBehavior(IEnumerable<IChapter> chapters)
        {
            Data.Chapters = chapters.ToList();
            Data.Name = "Step Group";
        }

        public ExecuteChaptersBehavior(IChapter chapter)
        {
            Data.Chapters = new List<IChapter> { chapter };
            Data.Name = "Step Group";
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach(IChapter chapter in Data.Chapters)
                {
                    chapter.LifeCycle.Activate();
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while(Data.Chapters.Select(chapter => chapter.LifeCycle.Stage).Any(stage => stage != Stage.Active)) 
                {
                    foreach (IChapter chapter in Data.Chapters.Where(chapter => chapter.LifeCycle.Stage != Stage.Active))
                    {
                        chapter.Update();
                    }
                    yield return null;
                }
            }

            /// <inheritdoc />
            public override void End()
            {
            }

            /// <inheritdoc />
            public override void FastForward()
            {
                //if (Data.Chapter.Data.Current == null)
                //{
                //    Data.Chapter.Data.Current = Data.Chapter.Data.FirstStep;
                //}

                //Data.Chapter.LifeCycle.MarkToFastForwardStage(Stage.Activating);
            }
        }

        private class DeactivatingProcess : StageProcess<EntityData>
        {
            public DeactivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (IChapter chapter in Data.Chapters)
                {
                    chapter.LifeCycle.Deactivate();
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while (Data.Chapters.Select(chapter => chapter.LifeCycle.Stage).Any(stage => stage != Stage.Inactive))
                {
                    foreach (IChapter chapter in Data.Chapters.Where(chapter => chapter.LifeCycle.Stage != Stage.Inactive))
                    {
                        chapter.Update();
                    }
                    yield return null;
                }
            }

            /// <inheritdoc />
            public override void End()
            {
            }

            /// <inheritdoc />
            public override void FastForward()
            {
                //Data.Chapter.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

        /// <inheritdoc />
        public override IStageProcess GetDeactivatingProcess()
        {
            return new DeactivatingProcess(Data);
        }

        /// <inheritdoc />
        public override IBehavior Clone()
        {
            ExecuteChaptersBehavior clonedBehavior = new ExecuteChaptersBehavior();

            List<IChapter> clonedChapters = new List<IChapter>();

            foreach(IChapter chapter in Data.Chapters)
            {
                clonedChapters.Add(chapter.Clone());
            }

            clonedBehavior.Data.Chapters = clonedChapters;
            return clonedBehavior;
        }
    }
}
