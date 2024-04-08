using System;

namespace VRBuilder.Core.Attributes
{
    /// <summary>
    /// Attribute for an obsolete property which has been replaced by a different property.
    /// </summary>
    public class LegacyPropertyAttribute : Attribute
    {
        /// <summary>
        /// Name of the property replacing this obsolete property.
        /// </summary>
        public string NewPropertyName { get; private set; }

        public LegacyPropertyAttribute(string newPropertyName)
        {
            NewPropertyName = newPropertyName;
        }
    }
}
