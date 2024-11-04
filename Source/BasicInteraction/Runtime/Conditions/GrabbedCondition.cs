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

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when a <see cref="IGrabbableProperty"/> is grabbed.
    /// </summary>
    [DataContract(IsReference = true)]
    public class GrabbedCondition : Condition<GrabbedCondition.EntityData>
    {
        [DisplayName("Grab Object")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Grabbable objects")]
            public MultipleScenePropertyReference<IGrabbableProperty> Targets { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Grab {Targets}";

            [DataMember]
            [DisplayName("Keep objects grabbable after step")]
            public bool KeepUnlocked = true;

            public Metadata Metadata { get; set; }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Complete()
            {
                IGrabbableProperty grabbableProperty = Data.Targets.Values.FirstOrDefault();

                if (grabbableProperty != null)
                {
                    grabbableProperty.FastForwardGrab();
                }
            }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            protected override bool CheckIfCompleted()
            {
                return Data.Targets.Values.Any(property => property.IsGrabbed);
            }

            public ActiveProcess(EntityData data) : base(data)
            {
            }
        }

        [JsonConstructor, Preserve]
        public GrabbedCondition() : this(Guid.Empty)
        {
        }

        public GrabbedCondition(Guid guid)
        {
            Data.Targets = new MultipleScenePropertyReference<IGrabbableProperty>(guid);
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