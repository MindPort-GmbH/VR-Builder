using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when an object from a given pool is snapped into a target snap zone.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/snap-object")]
    public class SnappedCondition : Condition<SnappedCondition.EntityData>
    {
        [DisplayName("Snap Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Snappable Objects")]
            public MultipleScenePropertyReference<ISnappableProperty> TargetObjects { get; set; }

            [DataMember]
            [DisplayName("Zone to snap into")]
            public SingleScenePropertyReference<ISnapZoneProperty> TargetSnapZone { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Snap {TargetObjects} in {TargetSnapZone}";

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                return Data.TargetObjects.Values.Any(snappable => snappable.IsSnapped && snappable.SnappedZone == Data.TargetSnapZone.Value);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                ISnappableProperty snappable = Data.TargetObjects.Values.FirstOrDefault(snappable => snappable.IsSnapped == false);

                if (snappable != null && Data.TargetSnapZone.Value.IsObjectSnapped == false)
                {
                    snappable.FastForwardSnapInto(Data.TargetSnapZone.Value);
                }
            }
        }

        private class EntityConfigurator : Configurator<EntityData>
        {
            public EntityConfigurator(EntityData data) : base(data)
            {
            }

            public override void Configure(IMode mode, Stage stage)
            {
                Data.TargetSnapZone.Value.Configure(mode);
            }
        }

        [JsonConstructor, Preserve]
        public SnappedCondition() : this(Guid.Empty, Guid.Empty)
        {
        }

        public SnappedCondition(Guid targets, Guid snapZone)
        {
            Data.TargetObjects = new MultipleScenePropertyReference<ISnappableProperty>(targets);
            Data.TargetSnapZone = new SingleScenePropertyReference<ISnapZoneProperty>(snapZone);
        }

        public SnappedCondition(ISnappableProperty target, ISnapZoneProperty snapZone) : this(ProcessReferenceUtils.GetUniqueIdFrom(target), ProcessReferenceUtils.GetUniqueIdFrom(snapZone))
        {
        }

        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }

        protected override IConfigurator GetConfigurator()
        {
            return new EntityConfigurator(Data);
        }

        protected override IAutocompleter GetAutocompleter()
        {
            return new EntityAutocompleter(Data);
        }
    }
}