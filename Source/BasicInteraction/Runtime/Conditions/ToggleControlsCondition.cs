using System;
using System.Linq;
using System.Runtime.Serialization;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition that checks if toggle controls (e.g. switches) are set to a specified position.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ToggleControlsCondition<TProperty> : Condition<ToggleControlsCondition<TProperty>.EntityData> where TProperty : class, ISettableControlProperty<bool>
    {
        [DisplayName("Check Control Position")]
        public class EntityData : IConditionData
        {
            /// <summary>
            /// The controls to check.
            /// </summary>
            [DataMember]
            [DisplayName("Controls")]
            public MultipleScenePropertyReference<TProperty> ToggleControls { get; set; }

            /// <summary>
            /// The target position for the controls.
            /// </summary>
            [DataMember]
            [DisplayName("Target position")]
            public bool TargetPosition { get; set; }

            /// <summary>
            /// If true, the condition will be evaluated only when the controls are released.
            /// </summary>
            [DataMember]
            [DisplayName("Require release")]
            public bool RequireRelease { get; set; }

            /// <summary>
            /// If true, all controls have to be set to the specified value.
            /// Otherwise, only one control needs to be set for the condition to complete.
            /// </summary>
            [DataMember]
            [DisplayName("Set all controls")]
            public bool SetAllControls { get; set; }

            public bool IsCompleted { get; set; }

            [DataMember]
            [HideInProcessInspector]
            public string Name
            {
                get
                {
                    string details = "";
                    if (ToggleControls.Values?.Count() > 1)
                    {
                        details = SetAllControls ? " all" : " one of";
                    }

                    string target = TargetPosition ? "True" : "False";

                    return $"Set{details} {ToggleControls} to {target}";
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
                foreach (TProperty control in Data.ToggleControls.Values)
                {
                    control.FastForwardValue(Data.TargetPosition);
                }
            }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            protected override bool CheckIfCompleted()
            {
                if (Data.RequireRelease && Data.ToggleControls.Values.Any(control => control.IsInteracting))
                {
                    return false;
                }

                if (Data.SetAllControls)
                {
                    return Data.ToggleControls.Values.All(control => control.CurrentValue == Data.TargetPosition);
                }
                else
                {
                    return Data.ToggleControls.Values.Any(control => control.CurrentValue == Data.TargetPosition);
                }
            }

            public ActiveProcess(EntityData data) : base(data)
            {
            }
        }

        public ToggleControlsCondition() : this(Guid.Empty)
        {
        }

        public ToggleControlsCondition(ISettableControlProperty<float> control, bool targetPosition, bool requireRelease = false) : this(ProcessReferenceUtils.GetUniqueIdFrom(control), targetPosition, requireRelease)
        {
        }

        public ToggleControlsCondition(Guid control, bool targetPosition = false, bool requireRelease = false)
        {
            Data.ToggleControls = new MultipleScenePropertyReference<TProperty>(control);
            Data.TargetPosition = targetPosition;
            Data.RequireRelease = requireRelease;
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