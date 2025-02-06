using Newtonsoft.Json;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Behavior that moves target SceneObject to the position and rotation of another TargetObject.
    /// It takes `Duration` seconds, even if the target was in the place already.
    /// If `Duration` is equal or less than 0, transition is instantaneous.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/move-object")]
    public class MoveObjectBehavior : Behavior<MoveObjectBehavior.EntityData>
    {
        /// <summary>
        /// The "move object" behavior's data.
        /// </summary>
        [DisplayName("Move Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Target scene object to be moved.
            /// </summary>
            [DataMember]
            [DisplayName("Object")]
            public SingleSceneObjectReference TargetObject { get; set; }

            /// <summary>
            /// Target's position and rotation is linearly interpolated to match PositionProvider's position and rotation at the end of transition.
            /// </summary>
            [DataMember]
            [DisplayName("Final position provider")]
            public SingleSceneObjectReference FinalPosition { get; set; }

            /// <summary>
            /// Duration of the transition. If duration is equal or less than zero, target object movement is instantaneous.
            /// </summary>
            [DataMember]
            [DisplayName("Animation (in seconds)")]
            public float Duration { get; set; }

            [DataMember]
            [DisplayName("Animation curve")]
            public AnimationCurve AnimationCurve { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name => $"Move {TargetObject} to {FinalPosition}";
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            private float startingTime;

            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                startingTime = Time.time;

                RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(Data.TargetObject.Value);

                Rigidbody movingRigidbody = Data.TargetObject.Value.GameObject.GetComponent<Rigidbody>();
                if (movingRigidbody != null && movingRigidbody.isKinematic == false)
                {
#if UNITY_6000
                    movingRigidbody.linearVelocity = Vector3.zero;
#else
                    movingRigidbody.velocity = Vector3.zero;
#endif
                    movingRigidbody.angularVelocity = Vector3.zero;
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                Transform movingTransform = Data.TargetObject.Value.GameObject.transform;
                Transform targetPositionTransform = Data.FinalPosition.Value.GameObject.transform;

                Vector3 initialPosition = movingTransform.position;
                Quaternion initialRotation = movingTransform.rotation;

                while (Time.time - startingTime < Data.Duration)
                {
                    RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(Data.TargetObject.Value);

                    float progress = (Time.time - startingTime) / Data.Duration;

                    movingTransform.position = initialPosition + (targetPositionTransform.position - initialPosition) * Data.AnimationCurve.Evaluate(progress);
                    movingTransform.rotation = Quaternion.Euler(initialRotation.eulerAngles + (targetPositionTransform.rotation.eulerAngles - initialRotation.eulerAngles) * Data.AnimationCurve.Evaluate(progress));

                    yield return null;
                }
            }

            /// <inheritdoc />
            public override void End()
            {
                RuntimeConfigurator.Configuration.SceneObjectManager.RequestAuthority(Data.TargetObject.Value);

                Transform movingTransform = Data.TargetObject.Value.GameObject.transform;
                Transform targetPositionTransform = Data.FinalPosition.Value.GameObject.transform;

                movingTransform.position = targetPositionTransform.position;
                movingTransform.rotation = targetPositionTransform.rotation;

                Rigidbody movingRigidbody = Data.TargetObject.Value.GameObject.GetComponent<Rigidbody>();
                if (movingRigidbody != null && movingRigidbody.isKinematic == false)
                {
#if UNITY_6000
                    movingRigidbody.linearVelocity = Vector3.zero;
#else
                    movingRigidbody.velocity = Vector3.zero;
#endif                    
                    movingRigidbody.angularVelocity = Vector3.zero;
                }
            }

            public override void FastForward()
            {
            }
        }

        [JsonConstructor, Preserve]
        public MoveObjectBehavior() : this(Guid.Empty, Guid.Empty, 0f)
        {
        }

        public MoveObjectBehavior(ISceneObject target, ISceneObject positionProvider, float duration) : this(ProcessReferenceUtils.GetUniqueIdFrom(target), ProcessReferenceUtils.GetUniqueIdFrom(positionProvider), duration)
        {
        }

        public MoveObjectBehavior(Guid targetObjectId, Guid finalPositionId, float duration)
        {
            Data.TargetObject = new SingleSceneObjectReference(targetObjectId);
            Data.FinalPosition = new SingleSceneObjectReference(finalPositionId);
            Data.Duration = duration;
            Data.AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
