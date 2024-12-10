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
            private List<SubChapter> subChapters;

            /// <summary>
            /// SubChapters to be executed in parallel.
            /// </summary>
            [DataMember]
            public List<SubChapter> SubChapters { get; set; }

            /// <summary>
            /// If true, the chapter with the corresponding index can be interrupted
            /// if all other chapters are complete.
            /// </summary>
            [DataMember]
            public List<bool> IsOptionalChapter { get; set; }

            [IgnoreDataMember]
            public string Name => "Execute Chapters";

            public override IEnumerable<IChapter> GetChildren()
            {
                return SubChapters.Select(sc => sc.Chapter);
            }
        }

        [JsonConstructor, Preserve]
        public ExecuteChaptersBehavior() : this(chapters: new List<IChapter>())
        {
        }

        public ExecuteChaptersBehavior(IEnumerable<SubChapter> subChapters)
        {
            Data.SubChapters = new List<SubChapter>(subChapters);
        }

        public ExecuteChaptersBehavior(IEnumerable<IChapter> chapters) : this(new List<SubChapter>(chapters.Select(chapter => new SubChapter(chapter))))
        {
        }

        public ExecuteChaptersBehavior(IChapter chapter) : this(new List<SubChapter>() { new SubChapter(chapter) })
        {
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (SubChapter subChapter in Data.SubChapters)
                {
                    subChapter.Chapter.LifeCycle.Activate();
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while (Data.SubChapters.Any(sc => sc.IsOptional == false && sc.Chapter.LifeCycle.Stage != Stage.Active))
                {
                    foreach (SubChapter sc in Data.SubChapters.Where(sc => sc.Chapter.LifeCycle.Stage == Stage.Activating))
                    {
                        sc.Chapter.Update();
                    }

                    yield return null;
                }

                foreach (SubChapter subChapter in Data.SubChapters.Where(sc => sc.IsOptional && sc.Chapter.LifeCycle.Stage == Stage.Activating))
                {
                    subChapter.Chapter.LifeCycle.Abort();
                }

                while (Data.SubChapters.Any(sc => sc.Chapter.LifeCycle.Stage == Stage.Aborting))
                {
                    foreach (SubChapter sc in Data.SubChapters.Where(sc => sc.Chapter.LifeCycle.Stage == Stage.Aborting))
                    {
                        sc.Chapter.Update();
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
                foreach (SubChapter subChapter in Data.SubChapters)
                {
                    if (subChapter.Chapter.Data.Current == null)
                    {
                        subChapter.Chapter.Data.Current = subChapter.Chapter.Data.FirstStep;
                    }

                    subChapter.Chapter.LifeCycle.MarkToFastForwardStage(Stage.Activating);
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
                foreach (SubChapter subChapter in Data.SubChapters.Where(sc => sc.Chapter.LifeCycle.Stage != Stage.Inactive && sc.Chapter.LifeCycle.Stage != Stage.Aborting))
                {
                    subChapter.Chapter.LifeCycle.Deactivate();
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while (Data.SubChapters.Any(sc => sc.IsOptional == false && sc.Chapter.LifeCycle.Stage != Stage.Inactive))
                {
                    foreach (SubChapter subChapter in Data.SubChapters.Where(sc => sc.Chapter.LifeCycle.Stage != Stage.Inactive))
                    {
                        subChapter.Chapter.Update();
                    }

                    yield return null;
                }

                foreach (SubChapter subChapter in Data.SubChapters.Where(sc => sc.IsOptional && sc.Chapter.LifeCycle.Stage == Stage.Deactivating))
                {
                    subChapter.Chapter.LifeCycle.Abort();
                }

                while (Data.SubChapters.Any(sc => sc.Chapter.LifeCycle.Stage == Stage.Aborting))
                {
                    foreach (SubChapter sc in Data.SubChapters.Where(sc => sc.Chapter.LifeCycle.Stage == Stage.Aborting))
                    {
                        sc.Chapter.Update();
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
                foreach (SubChapter subChapter in Data.SubChapters)
                {
                    subChapter.Chapter.LifeCycle.MarkToFastForwardStage(Stage.Deactivating);
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
            Data.SubChapters.ForEach(sc => clonedBehavior.Data.SubChapters.Add(new SubChapter(sc.Chapter.Clone(), sc.IsOptional)));
            return clonedBehavior;
        }
    }
}
