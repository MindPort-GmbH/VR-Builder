using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

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
            public PropertyReferenceOrTagSelectableValue<IParticleSystemProperty> Target { get; set; }

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
                    string property = Target.FirstValue.IsEmpty() ? "[NULL]" : Target.FirstValue.Value.SceneObject.GameObject.name;
                    string tag = SceneObjectTags.Instance.GetLabel(Target.SecondValue.Guid);
                    tag = string.IsNullOrEmpty(tag) ? "<none>" : tag;
                    string action = EmitParticles ? "start" : "stop";
                    action = Target.IsFirstValueSelected ? action + "s" : action;
                    string target = Target.IsFirstValueSelected ? property : $"Objects with tag {tag}";                    
                    return $"{target} {action} emitting particles";
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
                List<IParticleSystemProperty> targetProperties = new List<IParticleSystemProperty>();

                if(Data.Target.IsFirstValueSelected)
                {
                    targetProperties.Add(Data.Target.FirstValue.Value);
                }
                else
                {
                    targetProperties.AddRange(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetPropertyByTag<IParticleSystemProperty>(Data.Target.SecondValue.Guid));
                }

                if(Data.EmitParticles)
                {
                    targetProperties.ForEach(property => property.StartEmission());
                }
                else
                {
                    targetProperties.ForEach(property => property.StopEmission());
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
            Data.Target = new PropertyReferenceOrTagSelectableValue<IParticleSystemProperty>();
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}