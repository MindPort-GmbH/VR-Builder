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
    }
}
