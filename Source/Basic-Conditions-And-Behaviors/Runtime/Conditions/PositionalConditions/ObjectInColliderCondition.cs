using Newtonsoft.Json;
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
            public SingleSceneObjectReference TargetObject { get; set; }

            /// <summary>
            /// The collider with trigger to enter.
            /// </summary>
            [DataMember]
            [DisplayName("Collider")]
#if CREATOR_PRO
            [CheckForCollider]
            [ColliderAreTrigger]
#endif
            public SingleScenePropertyReference<ColliderWithTriggerProperty> TriggerProperty { get; set; }

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [HideInProcessInspector]
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    try
                    {
                        string targetObject = TargetObject.IsEmpty() ? "[NULL]" : TargetObject.Value.GameObject.name;
                        string triggerProperty = TriggerProperty.IsEmpty() ? "[NULL]" : TriggerProperty.Value.SceneObject.GameObject.name;

                        return $"Move {targetObject} in collider {triggerProperty}";
                    }
                    catch
                    {
                        return "Object in Collider";
                    }
                }
            }

            /// <inheritdoc />
#if CREATOR_PRO
            [OptionalValue]
#endif
            [DataMember]
            [DisplayName("Required seconds inside")]
            public float RequiredTimeInside { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }
        }

        [JsonConstructor, Preserve]
        public ObjectInColliderCondition() : this("", "")
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public ObjectInColliderCondition(ColliderWithTriggerProperty targetPosition, ISceneObject targetObject, float requiredTimeInTarget = 0)
            : this(ProcessReferenceUtils.GetNameFrom(targetPosition), ProcessReferenceUtils.GetNameFrom(targetObject), requiredTimeInTarget)
        {
        }

        public ObjectInColliderCondition(string targetPosition, string targetObject, float requiredTimeInTarget = 0)
        {
            // TODO Update parameters
            Data.TriggerProperty = new SingleScenePropertyReference<ColliderWithTriggerProperty>();
            Data.TargetObject = new SingleSceneObjectReference();
            Data.RequiredTimeInside = requiredTimeInTarget;
        }

        private class ActiveProcess : ObjectInTargetActiveProcess<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            protected override bool IsInside()
            {
                return Data.TriggerProperty.Value.IsTransformInsideTrigger(Data.TargetObject.Value.GameObject.transform);
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
                Data.TriggerProperty.Value.FastForwardEnter(Data.TargetObject.Value);
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
