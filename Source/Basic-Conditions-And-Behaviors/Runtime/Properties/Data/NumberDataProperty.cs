using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Float implementation of the <see cref="DataProperty{T}"/> class.
    /// </summary>
    public class NumberDataProperty : DataProperty<float>
    {
        [SerializeField]
        private float defaultValue;

        /// <inheritdoc/>
        public override float DefaultValue => defaultValue;

        /// <summary>
        /// Increases the value of the data property by a given amount.
        /// </summary>        
        public void IncreaseValue(float increase)
        {
            SetValue(GetValue() + increase);
        }
    }
}
