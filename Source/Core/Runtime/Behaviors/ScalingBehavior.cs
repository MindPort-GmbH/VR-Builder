using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            [DisplayName("Target Objects")]
            public MultipleSceneObjectReference Targets { get; set; }

            // Target scale.
            [DataMember]
            [DisplayName("Target Scale")]
            public Vector3 TargetScale { get; set; }

            // Duration of the animation in seconds.
            [DataMember]
            [DisplayName("Animation Duration (in seconds)")]
            public float Duration { get; set; }

            [DataMember]
            [DisplayName("Animation curve")]
            public AnimationCurve AnimationCurve { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name => $"Scale {Targets} to {TargetScale}";
        }

        [JsonConstructor, Preserve]
        public ScalingBehavior() : this(Array.Empty<ISceneObject>(), Vector3.one, 0f)
        {
        }

        public ScalingBehavior(IEnumerable<ISceneObject> targets, Vector3 targetScale, float duration)
        {
            Data.Targets = new MultipleSceneObjectReference(targets.Select(target => target.Guid));
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

                ISceneObject[] sceneObjects = Data.Targets.Values.ToArray();
                Transform[] scaledTransforms = sceneObjects.Select(so => so.GameObject.transform).ToArray();
                Vector3[] initialScales = scaledTransforms.Select(t => t.localScale).ToArray();

                //Transform scaledTransform = Data.Target.Value.GameObject.transform;

                //Vector3 initialScale = scaledTransform.localScale;

                while (Time.time - startedAt < Data.Duration)
                {
                    for (int i = 0; i < sceneObjects.Length; i++)
                    {
                        RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(sceneObjects[i]);

                        float progress = (Time.time - startedAt) / Data.Duration;
                        scaledTransforms[i].localScale = initialScales[i] + (Data.TargetScale - initialScales[i]) * Data.AnimationCurve.Evaluate(progress);
                        yield return null;
                    }
                }
            }

            /// <inheritdoc />
            public override void End()
            {
                foreach (ISceneObject sceneObject in Data.Targets.Values)
                {
                    RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(sceneObject);

                    Transform scaledTransform = sceneObject.GameObject.transform;
                    scaledTransform.localScale = Data.TargetScale;
                }
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