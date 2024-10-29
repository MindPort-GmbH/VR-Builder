using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
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

            [DataMember]
            [HideInProcessInspector]
            [Obsolete("Use TouchableProperties instead.")]
            [LegacyProperty(nameof(TouchableProperties))]
            public ScenePropertyReference<ITouchableProperty> TouchableProperty { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Touch {TouchableProperties}";

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
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

                if (property != null)
                {
                    property.FastForwardTouch();
                }
            }
        }

        [JsonConstructor, Preserve]
        public TouchedCondition() : this(Guid.Empty)
        {
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public TouchedCondition(ITouchableProperty target) : this(ProcessReferenceUtils.GetUniqueIdFrom(target))
        {
        }

        public TouchedCondition(Guid uniqueId)
        {
            Data.TouchableProperties = new MultipleScenePropertyReference<ITouchableProperty>(uniqueId);
        }

        [Obsolete("This constructor only supports guids and will be removed in the next major version.")]
        public TouchedCondition(string uniqueId) : this(Guid.Parse(uniqueId))
        {
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