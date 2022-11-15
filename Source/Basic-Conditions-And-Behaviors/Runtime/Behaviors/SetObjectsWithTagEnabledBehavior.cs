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
            /// The object to enable.
            /// </summary>
            [DataMember]
            [DisplayName("Tag")]
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
                foreach(ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Data.Tag.Guid))
                {
                    sceneObject.GameObject.SetActive(Data.SetEnabled);
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
                    foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Data.Tag.Guid))
                    {
                        sceneObject.GameObject.SetActive(!Data.SetEnabled);
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public SetObjectsWithTagEnabledBehavior() : this(Guid.Empty, false)
        {
        }

        public SetObjectsWithTagEnabledBehavior(bool setEnabled, string name = "Set Objects Enabled") : this(Guid.Empty, setEnabled, false, name)
        {
        }

        public SetObjectsWithTagEnabledBehavior(Guid tag, bool setEnabled, bool revertOnDeactivate = false, string name = "Set Objects Enabled")
        {
            Data.Tag = new SceneObjectTag<ISceneObject>(tag);
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
