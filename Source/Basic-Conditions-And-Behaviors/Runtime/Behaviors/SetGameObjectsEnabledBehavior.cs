using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Enables gameObject of target ISceneObject.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/enable-object")]
    public class SetGameObjectsEnabledBehavior : Behavior<SetGameObjectsEnabledBehavior.EntityData>
    {
        /// <summary>
        /// "Enable game object" behavior's data.
        /// </summary>
        [DisplayName("Enable Objects by Tag")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// The object to enable.
            /// </summary>
            [DataMember]
            [UsesSpecificProcessDrawer("SceneObjectTagDrawer")]
            [DisplayName("Tag")]
            public Guid Tag { get; set; }

            [DataMember]
            public bool SetEnabled { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            [DataMember]
            [DisplayName("Disable Object after step is complete")]
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
                foreach(ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Data.Tag))
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
                if (Data.DisableOnDeactivating)
                {
                    foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Data.Tag))
                    {
                        sceneObject.GameObject.SetActive(!Data.SetEnabled);
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public SetGameObjectsEnabledBehavior() : this("")
        {
        }

        /// <param name="targetObject">Object to enable.</param>
        public SetGameObjectsEnabledBehavior(ISceneObject targetObject) : this(ProcessReferenceUtils.GetNameFrom(targetObject))
        {
        }

        /// <param name="targetObject">Name of the object to enable.</param>
        public SetGameObjectsEnabledBehavior(string targetObject, string name = "Enable Object")
        {
            //Data.Target = new SceneObjectReference(targetObject);
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
