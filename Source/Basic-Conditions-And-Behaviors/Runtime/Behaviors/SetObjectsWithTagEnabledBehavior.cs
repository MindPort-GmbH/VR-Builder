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
    /// Sets enabled or disabled all objects with a given tag.
    /// </summary>
    [DataContract(IsReference = true)]
    [Obsolete("Use SetObjectsEnabledBehavior instead.")]
    public class SetObjectsWithTagEnabledBehavior : Behavior<SetObjectsWithTagEnabledBehavior.EntityData>
    {
        /// <summary>
        /// Behavior data for <see cref="SetObjectsWithTagEnabledBehavior"/>.
        /// </summary>
        [DisplayName("Enable Objects by Tag")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// The objects to enable or disable.
            /// </summary>
            [DataMember]
            [DisplayName("Objects")]
            public MultipleSceneObjectReference TargetObjects { get; set; }

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use TargetObjects instead.")]
            [LegacyProperty(nameof(TargetObjects))]
            public SceneObjectTag<ISceneObject> Tag { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public bool SetEnabled { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            [DataMember]
            [DisplayName("Revert after step is complete")]
            public bool RevertOnDeactivation { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string setEnabled = SetEnabled ? "Enable" : "Disable";
                    return $"{setEnabled} {TargetObjects}";
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
                    RuntimeConfigurator.Configuration.SceneObjectManager.SetSceneObjectActive(sceneObject, Data.SetEnabled);
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
                        RuntimeConfigurator.Configuration.SceneObjectManager.SetSceneObjectActive(sceneObject, !Data.SetEnabled);
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public SetObjectsWithTagEnabledBehavior() : this(Guid.Empty, false)
        {
        }

        public SetObjectsWithTagEnabledBehavior(bool setEnabled) : this(Guid.Empty, setEnabled, false)
        {
        }

        public SetObjectsWithTagEnabledBehavior(Guid objectId, bool setEnabled, bool revertOnDeactivate = false)
        {
            Data.TargetObjects = new MultipleSceneObjectReference(objectId);
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
