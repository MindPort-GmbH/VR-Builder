using Newtonsoft.Json;
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
            [DisplayName("Data Property")]
            public ScenePropertyReference<IDataPropertyBase> DataProperty { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
                Data.DataProperty.Value.ResetValue();
            }

            /// <inheritdoc />
            public override void FastForward()
            {
            }
        }

        [JsonConstructor, Preserve]
        public ResetValueBehavior() : this("")
        {
        }

        public ResetValueBehavior(string name) : this("", name)
        {
        }

        public ResetValueBehavior(string propertyName, string name = "Reset Value")
        {
            Data.DataProperty = new ScenePropertyReference<IDataPropertyBase>(propertyName);
        }

        public ResetValueBehavior(IDataPropertyBase property, string name = "Reset Value") : this(ProcessReferenceUtils.GetNameFrom(property), name)
        {
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
