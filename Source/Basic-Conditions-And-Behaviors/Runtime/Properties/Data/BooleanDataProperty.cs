using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Boolean implementation of the <see cref="DataProperty{T}"/> class.
    /// </summary>
    public class BooleanDataProperty : DataProperty<bool>
    {
        [SerializeField]
        private bool defaultValue;

        /// <inheritdoc/>
        public override bool DefaultValue => defaultValue;

        /// <summary>
        /// Changes the property's value from true to false and viceversa.
        /// </summary>
        public void InvertValue()
        {
            SetValue(GetValue() == false);
        }
    }
}
