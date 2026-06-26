using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine.Scripting;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when `GrabbableProperty` becomes ungrabbed.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://mindport-gmbh.github.io/VR-Builder-Documentation/articles/core/release-object-condition.html?utm_source=unity_editor&utm_medium=referral&utm_campaign=from_unity&utm_id=from_unity")]
    public class ReleasedCondition : Condition<ReleasedCondition.EntityData>
    {
        [DisplayName("Release Object")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Grabbable objects")]
            [DisplayTooltip("Objects that must be released to complete the condition.")]
            public MultipleScenePropertyReference<IGrabbableProperty> GrabbableProperties { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Release {GrabbableProperties}";

            [DataMember]
            [DisplayName("Keep objects grabbable after step")]
            [DisplayTooltip("If enabled, the objects remain grabbable after the step ends.")]
            public bool KeepUnlocked = true;

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                return Data.GrabbableProperties.Values.All(property => property.IsGrabbed == false);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                IGrabbableProperty grabbableProperty = Data.GrabbableProperties.Values.FirstOrDefault(property => property.IsGrabbed);

                if (grabbableProperty != null)
                {
                    grabbableProperty.FastForwardUngrab();
                }
            }
        }

        [JsonConstructor, Preserve]
        public ReleasedCondition() : this(Guid.Empty)
        {
        }

        public ReleasedCondition(IGrabbableProperty target) : this(ProcessReferenceUtils.GetUniqueIdFrom(target))
        {
        }

        public ReleasedCondition(Guid uniqueId)
        {
            Data.GrabbableProperties = new MultipleScenePropertyReference<IGrabbableProperty>(uniqueId);
        }

        public override IEnumerable<LockablePropertyData> GetLockableProperties()
        {
            IEnumerable<LockablePropertyData> references = base.GetLockableProperties();
            foreach (LockablePropertyData propertyData in references)
            {
                propertyData.EndStepLocked = !Data.KeepUnlocked;
            }

            return references;
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