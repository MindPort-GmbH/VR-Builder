using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Enables/disables all components of a given type on a given game object.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/enable-object")]
    public class SetComponentEnabledBehavior : Behavior<SetComponentEnabledBehavior.EntityData>
    {
        /// <summary>
        /// The behavior's data.
        /// </summary>
        [DisplayName("Set Component Enabled")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Object the target component is on.
            /// </summary>
            [DataMember]
            [HideInProcessInspector]
            public MultipleSceneObjectReference TargetObjects { get; set; }

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
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string setEnabled = SetEnabled ? "Enable" : "Disable";
                    string componentType = string.IsNullOrEmpty(ComponentType) ? "<none>" : ComponentType;
                    return $"{setEnabled} {componentType} for {TargetObjects}";
                }
            }
        }

        private class ActivatingProcess : InstantProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                foreach (ISceneObject sceneObject in Data.TargetObjects.Values)
                {
                    RuntimeConfigurator.Configuration.SceneObjectManager.SetComponentActive(sceneObject, Data.ComponentType, Data.SetEnabled);
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
                    foreach (ISceneObject sceneObject in Data.TargetObjects.Values)
                    {
                        RuntimeConfigurator.Configuration.SceneObjectManager.SetComponentActive(sceneObject, Data.ComponentType, !Data.SetEnabled);
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public SetComponentEnabledBehavior() : this(Guid.Empty, "", false, false)
        {
        }

        public SetComponentEnabledBehavior(bool setEnabled, string name = "Set Component Enabled") : this(Guid.Empty, "", setEnabled, false)
        {
        }

        public SetComponentEnabledBehavior(Guid objectId, string componentType, bool setEnabled, bool revertOnDeactivate)
        {
            Data.TargetObjects = new MultipleSceneObjectReference(objectId);
            Data.ComponentType = componentType;
            Data.SetEnabled = setEnabled;
            Data.RevertOnDeactivation = revertOnDeactivate;
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
