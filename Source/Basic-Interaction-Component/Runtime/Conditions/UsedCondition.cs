using Newtonsoft.Json;
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
using VRBuilder.Core.Validation;

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
#if CREATOR_PRO              
            [CheckForCollider]
#endif
            [DataMember]
            [DisplayName("Object")]
            public ScenePropertyReference<IUsableProperty> UsableProperty { get; set; }

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
                return Data.UsableProperty.Value.IsBeingUsed;
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                Data.UsableProperty.Value.FastForwardUse();
            }
        }

        [JsonConstructor, Preserve]
        public UsedCondition() : this("")
        {
        }

        public UsedCondition(IUsableProperty target, string name = null) : this(ProcessReferenceUtils.GetNameFrom(target), name)
        {
        }

        public UsedCondition(string target, string name = "Use Object")
        {
            Data.UsableProperty = new ScenePropertyReference<IUsableProperty>(target);
            Data.Name = name;
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