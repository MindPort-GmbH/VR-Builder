using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
            public string ComponentType { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public bool SetEnabled { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public bool RevertOnDeactivation { get; set; }

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
                IEnumerable<Component> components = Data.Target.Value.GameObject.GetComponents<Component>().Where(c => c.GetType().Name == Data.ComponentType);

                foreach(Component component in components)
                {
                    Type componentType = component.GetType();

                    if (componentType.GetProperty("enabled") != null)
                    {
                        componentType.GetProperty("enabled").SetValue(component, Data.SetEnabled, null);
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
                if (Data.RevertOnDeactivation)
                {
                    IEnumerable<Component> components = Data.Target.Value.GameObject.GetComponents<Component>().Where(c => c.GetType().Name == Data.ComponentType);

                    foreach (Component component in components)
                    {
                        Type componentType = component.GetType();

                        if (componentType.GetProperty("enabled") != null)
                        {
                            componentType.GetProperty("enabled").SetValue(component, !Data.SetEnabled, null);
                        }
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public EnableComponentBehavior() : this("")
        {
            Data.ComponentType = "";
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
