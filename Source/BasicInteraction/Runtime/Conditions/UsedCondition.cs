﻿using Newtonsoft.Json;
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
            [UsesSpecificProcessDrawer("MultiLineStringDrawer")]
            public String Description = "";

            [DataMember]
            [DisplayName("All Objects required to be used")]
            public bool UseAll = false;

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                if (Data.UseAll == false)
                {
                    return Data.UsableObjects.Values.Any(property => property.IsBeingUsed);
                }

                return Data.UsableObjects.Values.All(property => property.WasUsed);
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
                if (Data.UseAll == false)
                {
                    if (property != null)
                    {
                        property.FastForwardUse();
                    }
                }
                else
                {
                    foreach (var touchableProperty in Data.UsableObjects.Values)
                    {
                        touchableProperty.FastForwardUse();
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