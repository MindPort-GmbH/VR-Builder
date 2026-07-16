using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// String implementation of the <see cref="DataProperty{T}"/> class.
    /// </summary>
    public class TextDataProperty : DataProperty<string>
    {
        [Header("Settings")]
        [SerializeField]
        private string defaultValue;

        [Header("Events")]
        [SerializeField]
        private UnityEvent<string> valueChanged = new UnityEvent<string>();

        [SerializeField]
        private UnityEvent valueReset = new UnityEvent();

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

        /// <inheritdoc/>
        public override string DefaultValue => defaultValue;
    }
}
