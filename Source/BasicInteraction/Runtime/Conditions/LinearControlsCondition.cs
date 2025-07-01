using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.BasicInteraction.Conditions
{
    /// <summary>
    /// Condition that checks if linear controls (e.g. levers, sliders) are set to a specific range.
    /// </summary>
    [DataContract(IsReference = true)]
    public class LinearControlsCondition<TProperty> : Condition<LinearControlsCondition<TProperty>.EntityData> where TProperty : class, ISettableControlProperty<float>
    {
        [DisplayName("Check Control Position")]
        public class EntityData : IConditionData
        {
            /// <summary>
            /// The controls to check.
            /// </summary>
            [DataMember]
            [DisplayName("Controls")]
            public MultipleScenePropertyReference<TProperty> LinearControls { get; set; }

            /// <summary>
            /// The minimum position of the controls (0-1 range).
            /// </summary>
            [DataMember]
            [DisplayName("Min position")]
            public float MinPosition { get; set; }

            /// <summary>
            /// The maximum position of the controls (0-1 range).
            /// </summary>
            [DataMember]
            [DisplayName("Max position")]
            public float MaxPosition { get; set; }

            /// <summary>
            /// If true, the condition will be evaluated only when the controls are released.
            /// </summary>
            [DataMember]
            [DisplayName("Require release")]
            public bool RequireRelease { get; set; }

            /// <summary>
            /// If true, all controls have to be set within the specified range.
            /// Otherwise, only one control needs to be within range for the condition to complete.
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
                    if (LinearControls.Values?.Count() > 1)
                    {
                        details = SetAllControls ? " all" : " one of";
                    }

                    return $"Set{details} {LinearControls} between {MinPosition} and {MaxPosition}";
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
                foreach (ISettableControlProperty<float> control in Data.LinearControls.Values)
                {
                    control.FastForwardValue(Data.MinPosition + (Data.MaxPosition - Data.MinPosition) / 2);
                }
            }
        }

        private class ActiveProcess : BaseActiveProcessOverCompletable<EntityData>
        {
            protected override bool CheckIfCompleted()
            {
                if (Data.LinearControls.Values.Count() == 0)
                {
                    Debug.LogError($"No controls are set in {GetType().Name}. The condition will complete immediately.");
                    return true;
                }

                if (Data.MinPosition > Data.MaxPosition)
                {
                    Debug.LogError($"{GetType().Name} for object {Data.LinearControls} will never complete as the minimum value is greater than the maximum value.");
                }

                if (Data.RequireRelease && Data.LinearControls.Values.Any(control => control.IsInteracting))
                {
                    return false;
                }

                if (Data.SetAllControls)
                {
                    return Data.LinearControls.Values.All(IsWithinRange);
                }
                else
                {
                    return Data.LinearControls.Values.Any(IsWithinRange);
                }
            }

            private bool IsWithinRange(ISettableControlProperty<float> control)
            {
                return control.CurrentValue >= Data.MinPosition && control.CurrentValue <= Data.MaxPosition;
            }

            public ActiveProcess(EntityData data) : base(data)
            {
            }
        }

        public LinearControlsCondition() : this(Guid.Empty)
        {
        }

        public LinearControlsCondition(ISettableControlProperty<float> control, float minPosition, float maxPosition, bool requireRelease = false) : this(ProcessReferenceUtils.GetUniqueIdFrom(control), minPosition, maxPosition, requireRelease)
        {
        }

        public LinearControlsCondition(Guid control, float minPosition = 0, float maxPosition = 1, bool requireRelease = false)
        {
            Data.LinearControls = new MultipleScenePropertyReference<TProperty>(control);
            Data.MinPosition = minPosition;
            Data.MaxPosition = maxPosition;
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