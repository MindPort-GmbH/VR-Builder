using Newtonsoft.Json;
using System;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Interaction;
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
            public SingleScenePropertyReference<ISnappableProperty> TargetObject { get; set; }

            [DataMember]
            [DisplayName("Snap zone to unsnap")]
            public SingleScenePropertyReference<ISnapZoneProperty> TargetSnapZone { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc/>            
            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string snappedObject = "[NULL]";
                    string snapZone = "[NULL]";

                    if (TargetObject.HasValue() || TargetSnapZone.HasValue())
                    {
                        snappedObject = TargetObject.HasValue() ? TargetObject.ToString() : "snapped object";
                        snapZone = TargetSnapZone.HasValue() ? TargetSnapZone.ToString() : "its snap zone";
                    }

                    return $"Unsnap {snappedObject} from {snapZone}";
                }
            }
        }

        [JsonConstructor, Preserve]
        public UnsnapBehavior() : this(Guid.Empty, Guid.Empty)
        {
        }

        public UnsnapBehavior(ISnappableProperty snappedObject, ISnapZoneProperty snapZone) : this(ProcessReferenceUtils.GetUniqueIdFrom(snappedObject), ProcessReferenceUtils.GetUniqueIdFrom(snapZone))
        {
        }

        public UnsnapBehavior(Guid snappedObjectId, Guid snapZoneId)
        {
            Data.TargetObject = new SingleScenePropertyReference<ISnappableProperty>(snappedObjectId);
            Data.TargetSnapZone = new SingleScenePropertyReference<ISnapZoneProperty>(snapZoneId);
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                ISnapZoneProperty snapZoneProperty = null;

                if (Data.TargetSnapZone.Value != null && (Data.TargetSnapZone.Value.SnappedObject == Data.TargetObject.Value || Data.TargetObject.Value == null))
                {
                    snapZoneProperty = Data.TargetSnapZone.Value;
                }
                else if (Data.TargetSnapZone.Value == null && Data.TargetObject.Value != null && Data.TargetObject.Value.IsSnapped)
                {
                    snapZoneProperty = Data.TargetObject.Value.SnappedZone;
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
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
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
