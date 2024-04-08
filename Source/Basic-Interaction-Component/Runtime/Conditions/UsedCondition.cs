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
    /// Condition which becomes completed when UsableProperty is used.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/use-object")]
    public class UsedCondition : Condition<UsedCondition.EntityData>
    {
        [DisplayName("Use Object")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Objects")]
            public MultipleScenePropertyReference<IUsableProperty> UsableObjects { get; set; }

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use UsableObjects instead.")]
            [LegacyProperty(nameof(UsableObjects))]
            public ScenePropertyReference<IUsableProperty> UsableProperty { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Use {UsableObjects}";

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                return Data.UsableObjects.Values.Any(usable => usable.IsBeingUsed);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                Data.UsableObjects.Values.FirstOrDefault()?.FastForwardUse();
            }
        }

        [JsonConstructor, Preserve]
        public UsedCondition() : this(Guid.Empty)
        {
        }

        public UsedCondition(IUsableProperty target) : this(ProcessReferenceUtils.GetUniqueIdFrom(target))
        {
        }

        [Obsolete("This constructor will be removed in the next major version.")]
        public UsedCondition(string target)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(target, out guid);
            Data.UsableObjects = new MultipleScenePropertyReference<IUsableProperty>(guid);
        }

        public UsedCondition(Guid target)
        {
            Data.UsableObjects = new MultipleScenePropertyReference<IUsableProperty>(target);
        }

        public override IEnumerable<LockablePropertyData> GetLockableProperties()
        {
            IEnumerable<LockablePropertyData> references = base.GetLockableProperties();
            // Only if UseableProperty required grab, keep it unlocked.
            if (references.Any(data => data.Property is IGrabbableProperty))
            {
                foreach (LockablePropertyData propertyData in references)
                {
                    if (propertyData.Property is IGrabbableProperty || propertyData.Property is ITouchableProperty)
                    {
                        propertyData.EndStepLocked = false;
                    }
                }
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