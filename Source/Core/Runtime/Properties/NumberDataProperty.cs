using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Float implementation of the <see cref="DataProperty{T}"/> class.
    /// </summary>
    public class NumberDataProperty : DataProperty<float>
    {
        [Header("Settings")]
        [SerializeField]
        private float defaultValue;

        [Header("Events")]
        [SerializeField]
        private UnityEvent<float> valueChanged = new UnityEvent<float>();

        [SerializeField]
        private UnityEvent valueReset = new UnityEvent();

        /// <inheritdoc/>
        public override float DefaultValue => defaultValue;

        protected override void OnEnable()
        {
            base.OnEnable();

            valueChanged.AddListener(SetValue);
            valueReset.AddListener(ResetValue);
        }

        protected void OnDisable()
        {
            valueChanged.RemoveListener(SetValue);
            valueReset.RemoveListener(ResetValue);
        }

        /// <summary>
        /// Increases the value of the data property by a given amount.
        /// </summary>
        public void IncreaseValue(float increase)
        {
            SetValue(GetValue() + increase);
        }
    }
}
