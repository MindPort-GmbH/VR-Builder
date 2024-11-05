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
    /// A behavior that sets a data property to a specified value.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder-tutorials/states-data-add-on")]
    public class SetValueBehavior<T> : Behavior<SetValueBehavior<T>.EntityData>
    {
        /// <summary>
        /// The <see cref="SetValueBehavior{T}"/> behavior data.
        /// </summary>
        [DisplayName("Set Value")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            [DataMember]
            [DisplayName("Data Property")]
            public MultipleScenePropertyReference<IDataProperty<T>> DataProperties { get; set; }

            [DataMember]
            [DisplayName("Value")]
            public T NewValue { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name => $"Set {DataProperties} to {NewValue}";
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (IDataProperty<T> dataProperty in Data.DataProperties.Values)
                {
                    dataProperty.SetValue(Data.NewValue);
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
        public SetValueBehavior() : this(Guid.Empty, default)
        {
        }

        public SetValueBehavior(Guid propertyId, T value)
        {
            Data.DataProperties = new MultipleScenePropertyReference<IDataProperty<T>>(propertyId);
            Data.NewValue = value;
        }

        public SetValueBehavior(IDataProperty<T> property, T value) : this(ProcessReferenceUtils.GetUniqueIdFrom(property), value)
        {
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

        /// <summary>
        /// Constructs concrete types in order for them to be seen by IL2CPP's ahead of time compilation.
        /// </summary>
        private class AOTHelper
        {
            SetValueBehavior<float> flt = new SetValueBehavior<float>();
            SetValueBehavior<string> str = new SetValueBehavior<string>();
            SetValueBehavior<bool> bln = new SetValueBehavior<bool>();
        }
    }
}
