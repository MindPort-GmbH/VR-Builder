using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Sets enabled or disabled all objects with a given tag.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/enable-object")]
    public class SetObjectsWithTagEnabledBehavior : Behavior<SetObjectsWithTagEnabledBehavior.EntityData>
    {
        /// <summary>
        /// "Enable game object" behavior's data.
        /// </summary>
        [DisplayName("Enable Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Scene Object to add the tags from
            /// </summary>
            [DataMember]
            [DisplayName("Drag Object to add tags")]
            public SceneObjectReference AssignSceneObjectTags { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public bool SetEnabled { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            [DataMember]
            [DisplayName("Revert after step is complete")]
            public bool RevertOnDeactivation { get; set; }

            //TODO this is currently not used as it is included in UniqueNameReference.TagGuids
            [DataMember]
            [HideInProcessInspector]
            public List<SceneObjectTags.Tag> Tags { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    return $"Enable Scene Objects With Tags";
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
                foreach (var tag in Data.Tags)
                {
                    foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag.Guid))
                    {
                        RuntimeConfigurator.Configuration.SceneObjectManager.SetSceneObjectActive(sceneObject, Data.SetEnabled);
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
                    foreach (var tag in Data.Tags)
                    {
                        foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(tag.Guid))
                        {
                            RuntimeConfigurator.Configuration.SceneObjectManager.SetSceneObjectActive(sceneObject, !Data.SetEnabled);
                        }
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public SetObjectsWithTagEnabledBehavior() : this(new List<Guid>(), false)
        {
        }

        public SetObjectsWithTagEnabledBehavior(bool setEnabled) : this(new List<Guid>(), setEnabled, false)
        {
        }

        public SetObjectsWithTagEnabledBehavior(List<Guid> tagGuids, bool setEnabled, bool revertOnDeactivation = false)
        {
            Data.Tags = new List<SceneObjectTags.Tag>();
            foreach (var tagGuid in tagGuids)
            {
                SceneObjectTags.Tag tag = SceneObjectTags.Instance.Tags.Where(tag => tag.Guid == tagGuid).FirstOrDefault();
                if (tag != null)
                {
                    Data.Tags.Add(tag);
                }
            }
            Data.SetEnabled = setEnabled;
            Data.RevertOnDeactivation = revertOnDeactivation;
            Data.AssignSceneObjectTags = new SceneObjectReference("");
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
