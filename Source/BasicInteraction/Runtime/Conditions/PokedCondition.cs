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
    /// Condition which completes when a pokable object is poked.
    /// </summary>
    [DataContract(IsReference = true)]
    //[HelpLink("https://www.mindport.co/vr-builder/manual/default-conditions/poke-object")]
    public class PokedCondition : Condition<PokedCondition.EntityData>
    {
        [DisplayName("Poke Object")]
        public class EntityData : IConditionData
        {
            [DataMember]
            [DisplayName("Pokable objects")]
            public MultipleScenePropertyReference<IPokableProperty> PokableProperties { get; set; }

            public bool IsCompleted { get; set; }

            [IgnoreDataMember]
            [HideInProcessInspector]
            public string Name => $"Poke {PokableProperties}";

            [DataMember]
            [DisplayName("All objects required to be poked")]
            public bool MustPokeAllObjects = false;

            public Metadata Metadata { get; set; }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            private HashSet<IPokableProperty> pokedObjects = new HashSet<IPokableProperty>();

            public ActiveProcess(EntityData data) : base(data)
            {
            }

            protected override bool CheckIfCompleted()
            {
                if (Data.MustPokeAllObjects)
                {
                    foreach (IPokableProperty pokableProperty in Data.PokableProperties.Values)
                    {
                        if (pokableProperty.IsBeingPoked)
                        {
                            pokedObjects.Add(pokableProperty);
                        }
                    }

                    return pokedObjects.Count == Data.PokableProperties.Values.Count();
                }

                return Data.PokableProperties.Values.Any(property => property.IsBeingPoked);
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
