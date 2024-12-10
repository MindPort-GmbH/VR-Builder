using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

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
            /// Identifies the particle system property to control.
            /// </summary>
            [DataMember]
            public MultipleScenePropertyReference<IParticleSystemProperty> Targets { get; set; }

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
                    string action = EmitParticles ? "Start" : "Stop";
                    return $"{action} emitting particles on {Targets}";
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
                if (Data.EmitParticles)
                {
                    Data.Targets.Values.ToList().ForEach(property => property.StartEmission());
                }
                else
                {
                    Data.Targets.Values.ToList().ForEach(property => property.StopEmission());
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

        public ControlParticleEmissionBehavior(bool emitParticles)
        {
            Data.EmitParticles = emitParticles;
            Data.Targets = new MultipleScenePropertyReference<IParticleSystemProperty>();
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
