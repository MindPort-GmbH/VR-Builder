using System.Collections;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.EntityOwners;
using System.Collections.Generic;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Behavior that waits for `DelayTime` seconds before finishing its activation.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/delay")]
    public class ExecuteChapterBehavior : Behavior<ExecuteChapterBehavior.EntityData>
    {
        /// <summary>
        /// The data class for a delay behavior.
        /// </summary>
        [DisplayName("Step Group")]
        [DataContract(IsReference = true)]
        public class EntityData : EntityCollectionData<IChapter>, IBehaviorData
        {
            [DataMember]
            public IChapter Chapter { get; set; }

            public string Name { get; set; }

            public override IEnumerable<IChapter> GetChildren()
            {
                return new List<IChapter>() { Chapter };
            }
        }

        [JsonConstructor, Preserve]
        public ExecuteChapterBehavior() : this(null)
        {
        }

        public ExecuteChapterBehavior(IChapter chapter)
        {
            Data.Chapter = chapter;
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
                Data.Chapter.LifeCycle.Activate();
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                while (Data.Chapter.LifeCycle.Stage != Stage.Active)
                {
                    Data.Chapter.Update();
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
                Data.Chapter.LifeCycle.MarkToFastForward();
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
