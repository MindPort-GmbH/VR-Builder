using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.EntityOwners;

namespace VRBuilder.Core
{
    public class StepSequence : Entity<StepSequence.EntityData>, IStep
    {
        [DataContract(IsReference = true)]
        public class EntityData : EntityCollectionData<IStepChild>, IStepData, IStepSequenceData
        {
            ///<inheritdoc />
            [DataMember]
            [DrawingPriority(0)]
            [HideInProcessInspector]
            public string Name { get; set; }

            ///<inheritdoc />
            [DataMember]
            [DrawingPriority(1)]
            public string Description { get; set; }

            ///<inheritdoc />
            [DataMember]
            [HideInProcessInspector]
            public IBehaviorCollection Behaviors { get; set; }

            ///<inheritdoc />
            [DataMember]
            [HideInProcessInspector]
            public ITransitionCollection Transitions { get; set; }


            ///<inheritdoc />
            public override IEnumerable<IStepChild> GetChildren()
            {
                return new List<IStepChild>
                {
                    Behaviors,
                    Transitions
                };
            }

            ///<inheritdoc />
            [IgnoreDataMember]
            public IStepChild Current { get; set; }

            ///<inheritdoc />
            public IMode Mode { get; set; }

            public IChapter Chapter { get; set; }

            public EntityData()
            {
            }
        }

        public StepMetadata StepMetadata { get; set; }

        IStepData IDataOwner<IStepData>.Data => Data;
    }
}
