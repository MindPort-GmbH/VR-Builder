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
    /// Behavior that start/stops a particle system property.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ControlParticleEmissionBehavior : Behavior<ControlParticleEmissionBehavior.EntityData>
    {
        /// <summary>
        /// The <see cref="ControlParticleEmissionBehavior"/> behavior data.
        /// </summary>        
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// The particle system property to control.
            /// </summary>
            [DataMember]
            [DisplayName("Particle System Property")]
            public ScenePropertyReference<IParticleSystemProperty> ParticleSystemProperty { get; set; }

            /// <summary>
            /// If true, particle emission starts, else it stops.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public bool EmitParticles { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string property = ParticleSystemProperty.IsEmpty() ? "[NULL]" : ParticleSystemProperty.Value.SceneObject.GameObject.name;
                    string action = EmitParticles ? "starts" : "stops";
                    return $"{property} {action} emitting particles";
                }
            }
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                if(Data.EmitParticles)
                {
                    Data.ParticleSystemProperty.Value.StartEmission();
                }
                else
                {
                    Data.ParticleSystemProperty.Value.StopEmission();
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
        public ControlParticleEmissionBehavior() : this(default)
        {
        }

        public ControlParticleEmissionBehavior(bool emitParticles) : this("", emitParticles)
        {
        }

        public ControlParticleEmissionBehavior(string propertyName, bool emitParticles)
        {
            Data.ParticleSystemProperty = new ScenePropertyReference<IParticleSystemProperty>(propertyName);
            Data.EmitParticles = emitParticles;
        }

        public ControlParticleEmissionBehavior(IParticleSystemProperty property, bool emitParticles) : this(ProcessReferenceUtils.GetNameFrom(property), emitParticles)
        {
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

    }
}