using Newtonsoft.Json;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Behaviors
{
    /// <summary>
    /// This behavior unsnaps an object from a snapzone.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-behaviors/unsnap-object")]
    public class UnsnapBehavior : Behavior<UnsnapBehavior.EntityData>
    {
        [DisplayName("Unsnap")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            [DataMember]
            [DisplayName("Object to unsnap")]
            public SingleScenePropertyReference<ISnappableProperty> SnappedObject { get; set; }

            [DataMember]
            [DisplayName("Snap zone to unsnap")]
            public SingleScenePropertyReference<ISnapZoneProperty> SnapZone { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc/>            
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    try
                    {
                        string snappedObject = "[NULL]";
                        string snapZone = "[NULL]";

                        if (SnappedObject.IsEmpty() == false || SnapZone.IsEmpty() == false)
                        {
                            snappedObject = SnappedObject.IsEmpty() ? "snapped object" : SnappedObject.Value.SceneObject.GameObject.name;
                            snapZone = SnapZone.IsEmpty() ? "its snap zone" : SnapZone.Value.SceneObject.GameObject.name;
                        }

                        return $"Unsnap {snappedObject} from {snapZone}";
                    }
                    catch
                    {
                        return "Unsnap object";
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public UnsnapBehavior() : this("", "")
        {
        }

        public UnsnapBehavior(ISnappableProperty snappedObject, ISnapZoneProperty snapZone) : this(ProcessReferenceUtils.GetNameFrom(snappedObject), ProcessReferenceUtils.GetNameFrom(snapZone))
        {
        }

        public UnsnapBehavior(string snappedObjectName, string snapZoneName)
        {
            // TODO handle parameters
            Data.SnappedObject = new SingleScenePropertyReference<ISnappableProperty>();
            Data.SnapZone = new SingleScenePropertyReference<ISnapZoneProperty>();
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
                ISnapZoneProperty snapZoneProperty = null;

                if (Data.SnapZone.Value != null && (Data.SnapZone.Value.SnappedObject == Data.SnappedObject.Value || Data.SnappedObject.Value == null))
                {
                    snapZoneProperty = Data.SnapZone.Value;
                }
                else if (Data.SnapZone.Value == null && Data.SnappedObject.Value != null && Data.SnappedObject.Value.IsSnapped)
                {
                    snapZoneProperty = Data.SnappedObject.Value.SnappedZone;
                }

                if (snapZoneProperty != null)
                {
                    ISnapZone snapZone = snapZoneProperty.SnapZoneObject.GetComponent<ISnapZone>();

                    if (snapZone != null)
                    {
                        snapZone.ForceRelease();
                    }
                }
            }

            /// <inheritdoc />
            public override void FastForward()
            {
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
