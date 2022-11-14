using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Enables/disables all components of a given type on a given game object.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/enable-object")]
    public class SetComponentEnabledByTagBehavior : Behavior<SetComponentEnabledByTagBehavior.EntityData>
    {
        /// <summary>
        /// The behavior's data.
        /// </summary>
        [DisplayName("Set Component Enabled (Tag)")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Object the target component is on.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public SceneObjectTag<ISceneObject> TargetTag { get; set; }

            /// <summary>
            /// Type of components to interact with.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public string ComponentType { get; set; }

            /// <summary>
            /// If true, the component will be enabled, otherwise it will disabled.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public bool SetEnabled { get; set; }

            /// <summary>
            /// If true, the component will revert to its original state when the behavior deactivates.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public bool RevertOnDeactivation { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

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
                IEnumerable<Component> components = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Data.TargetTag.Guid)
                    .SelectMany(sceneObject => sceneObject.GameObject.GetComponents<Component>())
                    .Where(component => component.GetType().Name == Data.ComponentType);

                foreach (Component component in components)
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
                    IEnumerable<Component> components = RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Data.TargetTag.Guid)
                        .SelectMany(sceneObject => sceneObject.GameObject.GetComponents<Component>())
                        .Where(component => component.GetType().Name == Data.ComponentType);

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
        public SetComponentEnabledByTagBehavior() : this(Guid.Empty, "", false, false, "")
        {
        }

        public SetComponentEnabledByTagBehavior(bool setEnabled, string name = "Set Component Enabled") : this(Guid.Empty, "", setEnabled, false, name)
        {
        }

        public SetComponentEnabledByTagBehavior(Guid tagGuid, string componentType, bool setEnabled, bool revertOnDeactivate, string name = "Set Component Enabled")
        {
            Data.TargetTag = new SceneObjectTag<ISceneObject>(tagGuid);
            Data.ComponentType = componentType;
            Data.SetEnabled = setEnabled;
            Data.RevertOnDeactivation = revertOnDeactivate;
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
