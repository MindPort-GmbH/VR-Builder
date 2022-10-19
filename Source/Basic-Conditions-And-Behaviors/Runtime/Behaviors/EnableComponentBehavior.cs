using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Enables gameObject of target ISceneObject.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/enable-object")]
    public class EnableComponentBehavior : Behavior<EnableComponentBehavior.EntityData>
    {
        /// <summary>
        /// "Enable game object" behavior's data.
        /// </summary>
        [DisplayName("Enable Component")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// The object to enable.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public SceneObjectReference Target { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public List<int> DisabledComponents { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            [DataMember]
            [DisplayName("Disable Object after step is complete")]
            [HideInProcessInspector]
            public bool DisableOnDeactivating { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }
        }

        private class ActivatingProcess : InstantProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                Component[] components = Data.Target.Value.GameObject.GetComponents<Component>();
                for(int i = 0; i < components.Length; ++i)
                {
                    Type componentType = components[i].GetType();

                    if(componentType.GetProperty("enabled") != null)
                    {
                        componentType.GetProperty("enabled").SetValue(components[i], Data.DisabledComponents.Contains(i) == false, null);
                    }
                }
            }
        }

        private class DeactivatingProcess : InstantProcess<EntityData>
        {
            public DeactivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                //if (Data.DisableOnDeactivating)
                //{
                //    Data.Target.Value.GameObject.SetActive(false);
                //}
            }
        }

        [JsonConstructor, Preserve]
        public EnableComponentBehavior() : this("")
        {
            Data.DisabledComponents = new List<int>();
        }

        /// <param name="targetObject">Object to enable.</param>
        public EnableComponentBehavior(ISceneObject targetObject) : this(ProcessReferenceUtils.GetNameFrom(targetObject))
        {
        }

        /// <param name="targetObject">Name of the object to enable.</param>
        public EnableComponentBehavior(string targetObject, string name = "Enable Object")
        {
            Data.Target = new SceneObjectReference(targetObject);
            Data.Name = name;
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

        public override IStageProcess GetDeactivatingProcess()
        {
            return new DeactivatingProcess(Data);
        }
    }
}
