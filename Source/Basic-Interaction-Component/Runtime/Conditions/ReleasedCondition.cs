using Newtonsoft.Json;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Validation;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when `GrabbableProperty` becomes ungrabbed.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/release-object")]
    public class ReleasedCondition : Condition<ReleasedCondition.EntityData>
    {
        [DisplayName("Release Object")]
        public class EntityData : IConditionData
        {
#if CREATOR_PRO
            [CheckForCollider]
#endif
            [DataMember]
            [DisplayName("Object")]
            public ScenePropertyReference<IGrabbableProperty> GrabbableProperty { get; set; }

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
                return Data.GrabbableProperty.Value.IsGrabbed == false;
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                Data.GrabbableProperty.Value.FastForwardUngrab();
            }
        }

        [JsonConstructor, Preserve]
        public ReleasedCondition() : this("")
        {
        }

        public ReleasedCondition(IGrabbableProperty target, string name = null) : this(ProcessReferenceUtils.GetNameFrom(target), name)
        {
        }

        public ReleasedCondition(string target, string name = "Release Object")
        {
            Data.GrabbableProperty = new ScenePropertyReference<IGrabbableProperty>(target);
            Data.Name = name;
        }

        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }

        protected override IAutocompleter GetAutocompleter()
        {
            return new EntityAutocompleter(Data);
        }
    }
}