using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            [IgnoreDataMember]
            [HideInProcessInspector]
            public readonly List<GameObject> Collected = new();

            /// <summary>
            /// The objects that can be placed in the collider.
            /// </summary>
            [DataMember]
            [DisplayName("Object")]
            public MultipleSceneObjectReference TargetObjects { get; set; }

            /// <summary>
            /// The colliders with trigger to enter.
            /// </summary>
            [DataMember]
            [DisplayName("Collider")]
            public SingleScenePropertyReference<ColliderWithTriggerProperty> TriggerObject { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            [HideInProcessInspector]
            [IgnoreDataMember]
            public string Name => $"Place one of {TargetObjects} in {TriggerObject}";
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
                //Selects one random element of the TargetObjects to move it to the TriggerObject position
                ISceneObject randomObject = Data.TargetObjects.Values.Where(p => Data.Collected.All(p2 => p2.gameObject != p.GameObject)).OrderBy(_ => Random.value).First();
                randomObject.GameObject.transform.position = Data.TriggerObject.Value.gameObject.transform.position;
            }
        }

        public class ActiveProcess: StageProcess<EntityData>
        {
            public override void Start()
            {
                Data.TriggerObject.Value.EnteredTrigger += ProcessEnterCollision;
                Data.IsCompleted = false;
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
            }

            public override void FastForward()
            {
            }

            // Declare the constructor. It calls the base method to bind the data object with the process.
            public ActiveProcess(EntityData data): base(data)
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
        }

        public override IStageProcess GetActiveProcess()
        {
            // Always return a new instance.
            return new ActiveProcess(Data);
        }

        protected override IAutocompleter GetAutocompleter()
        {
            // Always return a new instance.
            return new CollectedConditionAutocompleter(Data);
        }
    }
}