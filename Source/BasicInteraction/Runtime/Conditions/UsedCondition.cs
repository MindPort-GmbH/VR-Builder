using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
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
    /// Condition which becomes completed when UsableProperties are used.
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

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Use {UsableObjects}";

            [DataMember]
            [DisplayName("All Objects required to be used")]
            public bool MustUseAllObjects = false;

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            // Housekeeping for use all option to track objects have been used so far in this condition
            private HashSet<IUsableProperty> usedObjects = new HashSet<IUsableProperty>();

            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                if (Data.MustUseAllObjects)
                {
                    foreach (IUsableProperty usableProperty in Data.UsableObjects.Values)
                    {
                        if (usableProperty.IsBeingUsed)
                        {
                            usedObjects.Add(usableProperty);
                        }
                    }       
                    return usedObjects.Count == Data.UsableObjects.Values.Count();
                }
                return Data.UsableObjects.Values.Any(property => property.IsBeingUsed);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                IUsableProperty property =  Data.UsableObjects.Values.FirstOrDefault();
                if (Data.MustUseAllObjects == false)
                {
                    if (property != null)
                    {
                        property.FastForwardUse();
                    }
                }
                else
                {
                    foreach (var usableProperty in Data.UsableObjects.Values)
                    {
                        usableProperty.FastForwardUse();
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public UsedCondition() : this(Guid.Empty)
        {
        }

        public UsedCondition(IUsableProperty target) : this(ProcessReferenceUtils.GetUniqueIdFrom(target))
        {
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