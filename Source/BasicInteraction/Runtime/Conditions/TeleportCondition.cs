using Newtonsoft.Json;
using System;
using System.Linq;
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
    [HelpLink("https://mindport-gmbh.github.io/VR-Builder-Documentation/articles/core/teleport-condition.html?utm_source=unity_editor&utm_medium=referral&utm_campaign=from_unity&utm_id=from_unity")]
    public class TeleportCondition : Condition<TeleportCondition.EntityData>
    {
        [DisplayName("Teleport")]
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Teleportation Points")]
            [DisplayTooltip("Teleportation points the user must teleport to.")]
            public MultipleScenePropertyReference<ITeleportationProperty> TeleportationPoints { get; set; }

            /// <inheritdoc />
            public bool IsCompleted { get; set; }

            /// <inheritdoc />
            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Teleport to {TeleportationPoints}";

            /// <inheritdoc />
            public Metadata Metadata { get; set; }
        }

        [JsonConstructor, Preserve]
        public TeleportCondition() : this(Guid.Empty)
        {
        }

        public TeleportCondition(ITeleportationProperty teleportationPoint) : this(ProcessReferenceUtils.GetUniqueIdFrom(teleportationPoint))
        {
        }

        public TeleportCondition(Guid teleportationPoint)
        {
            Data.TeleportationPoints = new MultipleScenePropertyReference<ITeleportationProperty>(teleportationPoint);
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

                foreach (ITeleportationProperty teleportationProperty in Data.TeleportationPoints.Values)
                {
                    teleportationProperty.Initialize();
                }
            }

            /// <inheritdoc />
            protected override bool CheckIfCompleted()
            {
                return Data.TeleportationPoints.Values.Any(teleportationPoint => teleportationPoint.WasUsedToTeleport);
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
                Data.TeleportationPoints.Values.FirstOrDefault()?.FastForwardTeleport();
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
