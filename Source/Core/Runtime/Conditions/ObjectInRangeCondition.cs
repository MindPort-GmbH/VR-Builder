using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Conditions
{
    /// <summary>
    /// Condition that is completed when distance between `Target` and `TransformInRangeDetector` is closer than `range` units.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/object-nearby")]
    public class ObjectInRangeCondition : Condition<ObjectInRangeCondition.EntityData>
    {
        /// <summary>
        /// The data of "object in range" condition.
        /// </summary>
        [DisplayName("Object Nearby")]
        public class EntityData : IObjectInTargetData
        {
            /// <summary>
            /// The tracked objects.
            /// </summary>
            [DataMember]
            [DisplayName("Tracked object")]
            public SingleSceneObjectReference TargetObject { get; set; }

            /// <summary>
            /// The object to measure distance from.
            /// </summary>
            [DataMember]
            [DisplayName("Reference object")]
            public SingleScenePropertyReference<TransformInRangeDetectorProperty> ReferenceObject { get; set; }

            /// <summary>
            /// The required distance between two objects to trigger the condition.
            /// </summary>
            [DataMember]
            public float Range { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Move {TargetObject} within {Range} units of {ReferenceObject}";

            /// <inheritdoc />
            [DataMember]
            [DisplayName("Required seconds inside")]
            public float RequiredTimeInside { get; set; }

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }
        }

        [JsonConstructor, Preserve]
        public ObjectInRangeCondition() : this(Guid.Empty, Guid.Empty, 0f)
        {
        }

        public ObjectInRangeCondition(ISceneObject target, TransformInRangeDetectorProperty detector, float range, float requiredTimeInTarget = 0)
            : this(ProcessReferenceUtils.GetUniqueIdFrom(target), ProcessReferenceUtils.GetUniqueIdFrom(detector), range, requiredTimeInTarget)
        {
        }

        public ObjectInRangeCondition(Guid targetId, Guid detector, float range, float requiredTimeInTarget = 0)
        {
            Data.TargetObject = new SingleSceneObjectReference(targetId);
            Data.ReferenceObject = new SingleScenePropertyReference<TransformInRangeDetectorProperty>(detector);
            Data.Range = range;
            Data.RequiredTimeInside = requiredTimeInTarget;
        }

        private class ActiveProcess : ObjectInTargetActiveProcess<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            public override void Start()
            {
                Data.ReferenceObject.Value.SetTrackedTransform(Data.TargetObject.Value.GameObject.transform);
                Data.ReferenceObject.Value.DetectionRange = Data.Range;

                base.Start();
            }

            /// <inheritdoc />
            protected override bool IsInside()
            {
                return Data.ReferenceObject.Value.IsTargetInsideRange();
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Complete()
            {
                Data.TargetObject.Value.GameObject.transform.position = Data.ReferenceObject.Value.gameObject.transform.position;
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }

        /// <inheritdoc />
        protected override IAutocompleter GetAutocompleter()
        {
            return new EntityAutocompleter(Data);
        }
    }
}
