using Newtonsoft.Json;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when a teleportation action was executed into the referenced <see cref="ITeleportationProperty"/>.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/teleport")]
    public class TeleportCondition : Condition<TeleportCondition.EntityData>
    {
        [DisplayName("Teleport")]
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Teleportation Point")]
#if CREATOR_PRO
            [CheckForCollider]
#endif
            public SingleScenePropertyReference<ITeleportationProperty> TeleportationPoint { get; set; }

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    try
                    {
                        string teleportationPoint = TeleportationPoint.IsEmpty() ? "[NULL]" : TeleportationPoint.Value.SceneObject.GameObject.name;
                        return $"Teleport to {teleportationPoint}";
                    }
                    catch
                    {
                        return "Teleport to Anchor";
                    }
                }
            }
            /// <inheritdoc />
            public Metadata Metadata { get; set; }
        }

        [JsonConstructor, Preserve]
        public TeleportCondition() : this("")
        {
        }

        public TeleportCondition(ITeleportationProperty teleportationPoint) : this(ProcessReferenceUtils.GetNameFrom(teleportationPoint))
        {
        }

        public TeleportCondition(string teleportationPoint)
        {
            // TODO Update parameters
            Data.TeleportationPoint = new SingleScenePropertyReference<ITeleportationProperty>();
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
                base.Start();
                Data.TeleportationPoint.Value.Initialize();
            }

            /// <inheritdoc />
            protected override bool CheckIfCompleted()
            {
                return Data.TeleportationPoint.Value.WasUsedToTeleport;
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Complete()
            {
                Data.TeleportationPoint.Value.FastForwardTeleport();
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }

        /// <inheritdoc />
        protected override IAutocompleter GetAutocompleter()
        {
            return new EntityAutocompleter(Data);
        }
    }
}
