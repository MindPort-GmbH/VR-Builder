using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.SceneObjects;
using UnityEngine;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Conditions
{
    /// <summary>
    /// A condition that checks if new selected objects are entering the selected collider.
    /// </summary>
    [DataContract(IsReference = true)]
    public class CollectedCondition: Condition<CollectedCondition.EntityData>
    {
        [DataContract(IsReference = true)]
        [DisplayName("Collect Condition")]
        public class EntityData: IConditionData
        {
            [HideInProcessInspector]
            public readonly List<GameObject> Collected = new();

            /// <summary>
            /// The objects that has to enter the collider.
            /// </summary>
            [DataMember]
            [DisplayName("Object")]
            public MultipleSceneObjectReference TargetObjects { get; set; }

            /// <summary>
            /// The collider with trigger to enter.
            /// </summary>
            [DataMember]
            [DisplayName("Collider")]
            public SingleScenePropertyReference<ColliderWithTriggerProperty> TriggerObject { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [HideInProcessInspector]
            [IgnoreDataMember]
            public string Name => $"Collect unique {TargetObjects} in {TriggerObject}";
            public bool IsCompleted { get; set; }

            public EntityData()
            {
                TargetObjects = new MultipleSceneObjectReference();
                TriggerObject = new SingleScenePropertyReference<ColliderWithTriggerProperty>();
            }
        }

        public class CollectedConditionAutocompleter: Autocompleter<EntityData>
        {
            public CollectedConditionAutocompleter(EntityData data): base(data)
            {
            }

            public override void Complete()
            {
            }
        }

        public class ActivatingProcess: StageProcess<EntityData>
        {
            public override void Start()
            {
                Data.TriggerObject.Value.EnteredTrigger += ProcessEnterCollision;
                //Maybe remove collected objects if collision exit ?
                //Data.TriggerObject.Value.ExitedTrigger += ProcessExitCollision;
            }

            public override IEnumerator Update()
            {
                while (!Data.IsCompleted)
                {
                    //If no collision, wait for the next frame.
                    yield return null;
                }
            }

            public override void End()
            {
                Data.TriggerObject.Value.EnteredTrigger -= ProcessEnterCollision;
                Data.IsCompleted = false;
                //Data.TriggerObject.Value.ExitedTrigger -= ProcessExitCollision;
            }

            public override void FastForward()
            {
            }

            // Declare the constructor. It calls the base method to bind the data object with the process.
            public ActivatingProcess(EntityData data): base(data)
            {
            }

            private void ProcessEnterCollision(object sender, ColliderWithTriggerProperty.ColliderWithTriggerEventArgs c)
            {
                foreach (var sceneObject in Data.TargetObjects.Values)
                {
                    if (c.CollidedObject.gameObject == sceneObject.GameObject)
                    {
                        if (!Data.Collected.Contains(c.CollidedObject.gameObject))
                        {
                            Data.Collected.Add(c.CollidedObject.gameObject);
                            Data.IsCompleted = true;
                        }
                    }
                }
            }


            //private void ProcessExitCollision(object sender, ColliderWithTriggerProperty.ColliderWithTriggerEventArgs c)
            //{
            //    foreach (var sceneObject in Data.TargetObjects.Values)
            //    {
            //        if (c.CollidedObject.transform.GetInstanceID() == sceneObject.GameObject.GetInstanceID())
            //        {
            //            if (Data.Collected.Contains(c.CollidedObject.gameObject))
            //            {
            //                Data.Collected.Remove(c.CollidedObject.gameObject);
            //            }
            //        }
            //    }
            //}
        }

        public override IStageProcess GetActiveProcess()
        {
            // Always return a new instance.
            return new ActivatingProcess(Data);
        }

        protected override IAutocompleter GetAutocompleter()
        {
            // Always return a new instance.
            return new CollectedConditionAutocompleter(Data);
        }
    }
}