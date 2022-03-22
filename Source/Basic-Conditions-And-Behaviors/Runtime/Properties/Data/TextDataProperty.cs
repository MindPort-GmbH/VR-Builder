using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// String implementation of the <see cref="DataProperty{T}"/> class.
    /// </summary>
    public class TextDataProperty : DataProperty<string>
    {
        [SerializeField]
        private string defaultValue;

        /// <inheritdoc/>
        public override string DefaultValue => defaultValue;
    }
}
