using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.EntityOwners.ParallelEntityCollection;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Behavior that executes a number of chapters at the same time and completes when the chapters ends.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ExecuteChaptersBehavior : Behavior<ExecuteChaptersBehavior.EntityData>
    {
        /// <summary>
        /// Execute chapters behavior data.
        /// </summary>
        [DisplayName("Execute Chapters")]
        [DataContract(IsReference = true)]
        public class EntityData : EntityCollectionData<IChapter>, IBehaviorData
        {
            /// <summary>
            /// Chapters to be executed in parallel.
            /// </summary>
            [DataMember]
            public List<IChapter> Chapters { get; set; }

            /// <summary>
            /// If false, the chapter with the corresponding index can be interrupted
            /// if all other chapters are complete.
            /// </summary>
            [DataMember]
            public List<bool> IsBlockingChapter { get; set; }

            [IgnoreDataMember]
            public string Name => "Execute Chapters";

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
        }

        public ExecuteChaptersBehavior(IChapter chapter)
        {
            Data.Chapters = new List<IChapter> { chapter };
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (IChapter chapter in Data.Chapters)
                {
                    chapter.LifeCycle.Activate();
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while (Data.Chapters.Any(chapter => chapter.LifeCycle.Stage != Stage.Active))
                {
                    foreach (IChapter chapter in Data.Chapters.Where(chapter => chapter.LifeCycle.Stage == Stage.Activating))
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
                foreach (IChapter chapter in Data.Chapters)
                {
                    if (chapter.Data.Current == null)
                    {
                        chapter.Data.Current = chapter.Data.FirstStep;
                    }

                    chapter.LifeCycle.MarkToFastForwardStage(Stage.Activating);
                }
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
                foreach (IChapter chapter in Data.Chapters)
                {
                    chapter.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);
                }
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
        public override IStageProcess GetAbortingProcess()
        {
            return new ParallelAbortingProcess<EntityData>(Data);
        }

        /// <inheritdoc />
        public override IBehavior Clone()
        {
            ExecuteChaptersBehavior clonedBehavior = new ExecuteChaptersBehavior();
            Data.Chapters.ForEach(chapter => clonedBehavior.Data.Chapters.Add(chapter.Clone()));
            return clonedBehavior;
        }
    }
}
