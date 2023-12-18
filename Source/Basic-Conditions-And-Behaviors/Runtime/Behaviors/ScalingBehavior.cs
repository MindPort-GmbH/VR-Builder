using Newtonsoft.Json;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Behaviors
{
    // This behavior linearly changes scale of a Target object over Duration seconds, until it matches TargetScale.
    [DataContract(IsReference = true)]
    public class ScalingBehavior : Behavior<ScalingBehavior.EntityData>
    {
        [DisplayName("Scale Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            // Process object to scale.
            [DataMember]
            public SceneObjectReference Target { get; set; }

            // Target scale.
            [DataMember]
            [DisplayName("Target Scale")]
            public Vector3 TargetScale { get; set; }

            // Duration of the animation in seconds.
#if CREATOR_PRO
            [OptionalValue]
#endif
            [DataMember]
            [DisplayName("Animation Duration (in seconds)")]
            public float Duration { get; set; }

            [DataMember]
            [DisplayName("Animation curve")]
            public AnimationCurve AnimationCurve { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string target = Target.IsEmpty() ? "[NULL]" : Target.Value.GameObject.name;
                    return $"Scale {target} to {TargetScale.ToString()}";
                }
            }
        }

        [JsonConstructor, Preserve]
        public ScalingBehavior() : this(new SceneObjectReference(), Vector3.one, 0f)
        {
        }

        public ScalingBehavior(SceneObjectReference target, Vector3 targetScale, float duration)
        {
            Data.Target = target;
            Data.TargetScale = targetScale;
            Data.Duration = duration;
            Data.AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
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
                float startedAt = Time.time;

                Transform scaledTransform = Data.Target.Value.GameObject.transform;

                Vector3 initialScale = scaledTransform.localScale;

                while (Time.time - startedAt < Data.Duration)
                {
                    RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(Data.Target.Value);

                    float progress = (Time.time - startedAt) / Data.Duration;
                    scaledTransform.localScale = initialScale + (Data.TargetScale - initialScale) * Data.AnimationCurve.Evaluate(progress);
                    yield return null;
                }
            }

            /// <inheritdoc />
            public override void End()
            {
                RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(Data.Target.Value);

                Transform scaledTransform = Data.Target.Value.GameObject.transform;
                scaledTransform.localScale = Data.TargetScale;
            }

            /// <inheritdoc />
            public override void FastForward()
            {
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}