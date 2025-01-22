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
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition which is completed when TouchableProperty is touched.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/touch-object")]
    public class TouchedCondition : Condition<TouchedCondition.EntityData>
    {
        [DisplayName("Touch Object")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Touchable objects")]
            public MultipleScenePropertyReference<ITouchableProperty> TouchableProperties { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Touch {TouchableProperties}";

            [DataMember] [DisplayName("All Objects required to be touched")]
            public bool MustTouchAllObjects = false;


            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            // Housekeeping for touch all option to track objects have been touched so far in this condition
            private HashSet<ITouchableProperty> touchedObjects = new HashSet<ITouchableProperty>();
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                if (Data.MustTouchAllObjects)
                {
                    foreach (ITouchableProperty touchableProperty in Data.TouchableProperties.Values)
                    {
                        if (touchableProperty.IsBeingTouched)
                        {
                            touchedObjects.Add(touchableProperty);
                        }
                    }       
                    return touchedObjects.Count == Data.TouchableProperties.Values.Count();
                }
                return Data.TouchableProperties.Values.Any(property => property.IsBeingTouched);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                ITouchableProperty property = Data.TouchableProperties.Values.FirstOrDefault();

                if (Data.MustTouchAllObjects == false)
                {
                    if (property != null)
                    {
                        property.FastForwardTouch();
                    }
                }
                else
                {
                    foreach (var touchableProperty in Data.TouchableProperties.Values)
                    {
                        touchableProperty.FastForwardTouch();
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public TouchedCondition() : this(Guid.Empty)
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public TouchedCondition(ITouchableProperty target) :
            this(ProcessReferenceUtils.GetUniqueIdFrom(target))
        {
        }

        public TouchedCondition(Guid uniqueId)
        {
            Data.TouchableProperties = new MultipleScenePropertyReference<ITouchableProperty>(uniqueId);
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