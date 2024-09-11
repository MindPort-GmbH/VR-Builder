using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Conditions
{
    /// <summary>
    /// Condition which is completed when `TargetObject` gets inside `TriggerProperty`'s collider.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/move-object-in-collider")]
    public class ObjectInColliderCondition : Condition<ObjectInColliderCondition.EntityData>
    {
        /// <summary>
        /// The "object in collider" condition's data.
        /// </summary>
        [DisplayName("Move Object into Collider")]
        [DataContract(IsReference = true)]
        public class EntityData : IObjectInTargetData
        {
            /// <summary>
            /// The object that has to enter the collider.
            /// </summary>
            [DataMember]
            [DisplayName("Object")]
            public MultipleSceneObjectReference TargetObjects { get; set; }

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use TargetObjects instead.")]
            [LegacyProperty(nameof(TargetObjects))]
            public SceneObjectReference TargetObject { get; set; }

            /// <summary>
            /// The collider with trigger to enter.
            /// </summary>
            [DataMember]
            [DisplayName("Collider")]

            public SingleScenePropertyReference<ColliderWithTriggerProperty> TriggerObject { get; set; }

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use TriggerObject instead.")]
            [LegacyProperty(nameof(TriggerObject))]
            public ScenePropertyReference<ColliderWithTriggerProperty> TriggerProperty { get; set; }
            
            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [HideInProcessInspector]
            [IgnoreDataMember]
            public string Name => $"Move {TargetObjects} in collider {TriggerObject}";

            /// <inheritdoc />
            [DataMember]
            [DisplayName("Required seconds inside")]
            public float RequiredTimeInside { get; set; }
            
            [DataMember]
            [DisplayName("Required Object count")]
            public float CheckValue { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }
        }

        [JsonConstructor, Preserve]
        public ObjectInColliderCondition() : this(Guid.Empty, Guid.Empty)
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public ObjectInColliderCondition(ColliderWithTriggerProperty targetPosition, ISceneObject targetObject, float requiredTimeInTarget = 0)
            : this(ProcessReferenceUtils.GetUniqueIdFrom(targetPosition), ProcessReferenceUtils.GetUniqueIdFrom(targetObject), requiredTimeInTarget)
        {
        }

        [Obsolete("This constructor will be removed in the next major version.")]
        public ObjectInColliderCondition(string targetPosition, string targetObject, float requiredTimeInTarget = 0, float checkValue = 1)
        {
            Guid triggerGuid = Guid.Empty;
            Guid targetGuid = Guid.Empty;
            Guid.TryParse(targetPosition, out triggerGuid);
            Guid.TryParse(targetObject, out targetGuid);
            Data.TriggerObject = new SingleScenePropertyReference<ColliderWithTriggerProperty>(triggerGuid);
            Data.TargetObjects = new MultipleSceneObjectReference(targetGuid);
            Data.RequiredTimeInside = requiredTimeInTarget;
            Data.CheckValue = checkValue;
        }

        public ObjectInColliderCondition(Guid targetPosition, Guid targetObject, float requiredTimeInTarget = 0, float checkValue = 1)
        {
            Data.TriggerObject = new SingleScenePropertyReference<ColliderWithTriggerProperty>(targetPosition);
            Data.TargetObjects = new MultipleSceneObjectReference(targetObject);
            Data.RequiredTimeInside = requiredTimeInTarget;
            Data.CheckValue = checkValue;
        }

        private class ActiveProcess : ObjectInTargetActiveProcess<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            protected override bool IsInside()
            {
                bool isTransformInsideTrigger = false;
                int counter = 0;

                foreach (ISceneObject sceneObject in Data.TargetObjects.Values)
                {
                    counter += Data.TriggerObject.Value.IsTransformInsideTrigger(sceneObject.GameObject.transform)? 1 : 0;
                }

                return counter >= Data.CheckValue;
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
                ISceneObject sceneObject = Data.TargetObjects.Values.FirstOrDefault();

                if (sceneObject != null)
                {
                    Data.TriggerObject.Value.FastForwardEnter(sceneObject);
                }
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
