using Newtonsoft.Json;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Validation;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when `Target` is snapped into `ZoneToSnapInto`.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/snap-object")]
    public class SnappedCondition : Condition<SnappedCondition.EntityData>
    {
        [DisplayName("Snap Object")]
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
#if CREATOR_PRO     
            [CheckForCollider]
#endif
            [DataMember]
            [DisplayName("Object")]
            public ScenePropertyReference<ISnappableProperty> Target { get; set; }

#if CREATOR_PRO        
            [CheckForCollider]
            [ColliderAreTrigger]
#endif
            [DataMember]
            [DisplayName("Zone to snap into")]
            public ScenePropertyReference<ISnapZoneProperty> ZoneToSnapInto { get; set; }

            public bool IsCompleted { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public string Name { get; set; }

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                return Data.Target.Value.IsSnapped && (Data.ZoneToSnapInto.Value == null || Data.ZoneToSnapInto.Value == Data.Target.Value.SnappedZone);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                Data.Target.Value.FastForwardSnapInto(Data.ZoneToSnapInto.Value);
            }
        }

        private class EntityConfigurator : Configurator<EntityData>
        {
            public EntityConfigurator(EntityData data) : base(data)
            {
            }

            public override void Configure(IMode mode, Stage stage)
            {
                Data.ZoneToSnapInto.Value.Configure(mode);
            }
        }

        [JsonConstructor, Preserve]
        public SnappedCondition() : this("", "")
        {
        }

        public SnappedCondition(ISnappableProperty target, ISnapZoneProperty snapZone = null, string name = null) : this(ProcessReferenceUtils.GetNameFrom(target), ProcessReferenceUtils.GetNameFrom(snapZone), name)
        {
        }

        public SnappedCondition(string target, string snapZone, string name = "Snap Object")
        {
            Data.Target = new ScenePropertyReference<ISnappableProperty>(target);
            Data.ZoneToSnapInto = new ScenePropertyReference<ISnapZoneProperty>(snapZone);
            Data.Name = name;
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