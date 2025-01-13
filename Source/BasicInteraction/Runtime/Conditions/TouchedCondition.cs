using System;
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

            [DataMember] [UsesSpecificProcessDrawer("MultiLineStringDrawer")]
            public String Description = "";

            [DataMember] [DisplayName("All Objects required to be touched")]
            public bool TouchAll = false;

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                if (Data.TouchAll == false)
                {
                    return Data.TouchableProperties.Values.Any(property => property.IsBeingTouched);
                }

                return Data.TouchableProperties.Values.All(property => property.WasTouched);
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

                if (Data.TouchAll == false)
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