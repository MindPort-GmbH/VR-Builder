using Newtonsoft.Json;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// A behavior that reset a data property to its default value specified in the inspector.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder-tutorials/states-data-add-on")]
    public class ResetValueBehavior : Behavior<ResetValueBehavior.EntityData>
    {
        /// <summary>
        /// The <see cref="ResetValueBehavior{T}"/> behavior data.
        /// </summary>
        [DisplayName("Reset Value")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            [DataMember]
            [DisplayName("Data Properties")]
            public MultipleScenePropertyReference<IDataPropertyBase> Properties;

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name => $"Reset {Properties} to default";
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (IDataPropertyBase dataProperty in Data.Properties.Values)
                {
                    dataProperty.ResetValue();
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

        [JsonConstructor, Preserve]
        public ResetValueBehavior() : this(Guid.Empty)
        {
        }

        public ResetValueBehavior(Guid propertyId)
        {
            Data.Properties = new MultipleScenePropertyReference<IDataPropertyBase>(propertyId);
        }

        public ResetValueBehavior(IDataPropertyBase property) : this(ProcessReferenceUtils.GetUniqueIdFrom(property))
        {
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
