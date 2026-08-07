using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Boolean implementation of the <see cref="DataProperty{T}"/> class.
    /// </summary>
    public class BooleanDataProperty : DataProperty<bool>
    {
        [Header("Settings")]
        [SerializeField]
        private bool defaultValue;

        [Header("Events")]
        [SerializeField]
        private UnityEvent<bool> valueChanged = new UnityEvent<bool>();

        [SerializeField]
        private UnityEvent valueReset = new UnityEvent();

        /// <inheritdoc/>
        public override bool DefaultValue => defaultValue;

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
        /// Changes the property's value from true to false and viceversa.
        /// </summary>
        public void InvertValue()
        {
            SetValue(!GetValue());
        }
    }
}
