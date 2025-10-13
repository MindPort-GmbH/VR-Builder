using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition that checks if one or more momentary controls (e.g. buttons) are triggered.
    /// </summary>
    [DataContract(IsReference = true)]
    public class MomentaryControlsCondition<TProperty> : Condition<MomentaryControlsCondition<TProperty>.EntityData> where TProperty : class, IMomentaryControlProperty
    {
        /// <summary>
        /// Data class for the condition.
        /// </summary>
        [DataContract(IsReference = true)]
        [DisplayName("Check Control Position")]
        public class EntityData : IConditionData
        {
            /// <summary>
            /// The controls to check.
            /// </summary>
            [DataMember]
            [DisplayName("Controls")]
            public MultipleScenePropertyReference<TProperty> MomentaryControls { get; set; }

            /// <summary>
            /// If true, the condition will complete when the controls are released. 
            /// Otherwise, it will complete when the controls are triggered.            
            /// </summary>
            [DataMember]
            [DisplayName("Trigger on release")]
            public bool TriggerOnRelease { get; set; }

            /// <summary>
            /// If true, the condition will complete when all controls are triggered.
            /// Otherwise, only one control needs to be triggered to complete the condition.
            /// </summary>
            [DataMember]
            [DisplayName("Trigger all controls")]
            public bool TriggerAllControls { get; set; }

            /// <inheritdoc/>
            public bool IsCompleted { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    string details = "";
                    if (MomentaryControls.Values?.Count() > 1)
                    {
                        details = TriggerAllControls ? " all" : " one of";
                    }

                    return $"Trigger{details} {MomentaryControls}";
                }
            }

            public Metadata Metadata { get; set; }
        }

        private class EntityAutocompleter : Autocompleter<EntityData>
        {
            public EntityAutocompleter(EntityData data) : base(data)
            {
            }

            public override void Complete()
            {
                foreach (IMomentaryControlProperty control in Data.MomentaryControls.Values)
                {
                    control.FastForwardTrigger();
                }
            }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            public override void Start()
            {
                base.Start();

                foreach (IMomentaryControlProperty control in Data.MomentaryControls.Values)
                {
                    control.Initialize();
                }
            }

            protected override bool CheckIfCompleted()
            {
                if (Data.MomentaryControls.Values.Count() == 0)
                {
                    Debug.LogError($"No controls are set in {GetType().Name}. The condition will complete immediately.");
                    return true;
                }

                if (Data.TriggerAllControls)
                {
                    return Data.MomentaryControls.Values.All(IsTriggered);
                }
                else
                {
                    return Data.MomentaryControls.Values.Any(IsTriggered);
                }
            }

            private bool IsTriggered(IMomentaryControlProperty control)
            {
                return Data.TriggerOnRelease ? control.HasBeenReleased : control.HasBeenTriggered;
            }

            public ActiveProcess(EntityData data) : base(data)
            {
            }
        }

        public MomentaryControlsCondition()
        {
            Data.MomentaryControls = new MultipleScenePropertyReference<TProperty>();
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