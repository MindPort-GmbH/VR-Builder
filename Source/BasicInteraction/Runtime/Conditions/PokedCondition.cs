using System;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;
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
    /// Condition which completes when a pokable object is poked to a configurable depth
    /// and held for a configurable duration.
    /// </summary>
    [DataContract(IsReference = true)]
    //[HelpLink("Later Documentation Link")]
    public class PokedCondition : Condition<PokedCondition.EntityData>
    {
        [DisplayName("Poke Object")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Pokable objects")]
            public MultipleScenePropertyReference<IPokableProperty> PokableProperties { get; set; }

            [DataMember]
            [DisplayName("All objects required to be poked")]
            public bool MustPokeAllObjects { get; set; }

            [DataMember]
            [DisplayName("Poke depth threshold (0-1)")]
            public float PokeDepthThreshold { get; set; }

            [DataMember]
            [DisplayName("Required hold duration (seconds)")]
            public float RequiredHoldDuration { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    string name = $"Poke {PokableProperties}";

                    if (PokeDepthThreshold > 0f)
                    {
                        name += $" (depth >= {PokeDepthThreshold:F1})";
                    }

                    if (RequiredHoldDuration > 0f)
                    {
                        name += $" for {RequiredHoldDuration:F1}s";
                    }

                    return name;
                }
            }

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : StageProcess<EntityData>
        {
            private bool isAtThreshold;
            private float timeThresholdReached;

            public ActiveProcess(EntityData data) : base(data)
            {
            }

            public override void Start()
            {
                Data.IsCompleted = false;
                isAtThreshold = CheckDepthMet();

                if (isAtThreshold)
                {
                    timeThresholdReached = Time.time;
                }
            }

            public override IEnumerator Update()
            {
                while (true)
                {
                    bool nowAtThreshold = CheckDepthMet();

                    if (nowAtThreshold != isAtThreshold)
                    {
                        isAtThreshold = nowAtThreshold;

                        if (isAtThreshold)
                        {
                            timeThresholdReached = Time.time;
                        }
                    }

                    if (isAtThreshold && Time.time - timeThresholdReached >= Data.RequiredHoldDuration)
                    {
                        Data.IsCompleted = true;
                        break;
                    }

                    yield return null;
                }
            }

            public override void End()
            {
            }

            public override void FastForward()
            {
            }

            private bool CheckDepthMet()
            {
                if (Data.MustPokeAllObjects)
                {
                    return Data.PokableProperties.Values.All(
                        property => property.IsBeingPoked && property.CurrentPokeDepth >= Data.PokeDepthThreshold);
                }

                return Data.PokableProperties.Values.Any(
                    property => property.IsBeingPoked && property.CurrentPokeDepth >= Data.PokeDepthThreshold);
            }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                if (Data.MustPokeAllObjects == false)
                {
                    IPokableProperty property = Data.PokableProperties.Values.FirstOrDefault();

                    if (property != null)
                    {
                        property.FastForwardPoke();
                    }
                }
                else
                {
                    foreach (var pokableProperty in Data.PokableProperties.Values)
                    {
                        pokableProperty.FastForwardPoke();
                    }
                }
            }
        }

        [JsonConstructor, Preserve]
        public PokedCondition() : this(Guid.Empty)
        {
        }

        public PokedCondition(IPokableProperty target) :
            this(ProcessReferenceUtils.GetUniqueIdFrom(target))
        {
        }

        public PokedCondition(Guid uniqueId)
        {
            Data.PokableProperties = new MultipleScenePropertyReference<IPokableProperty>(uniqueId);
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
