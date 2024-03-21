using System;

namespace VRBuilder.Core.Attributes
{
    public class LegacyPropertyAttribute : Attribute
    {
        public string NewPropertyName { get; private set; }

        public LegacyPropertyAttribute(string newPropertyName)
        {
            NewPropertyName = newPropertyName;
        }
    }
}
