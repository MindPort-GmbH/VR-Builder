using System;
using System.Reflection;

namespace VRBuilder.Core.Attributes
{
    /// <summary>
    /// Declares that "Menu" button has to be drawn.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MenuAttribute : MetadataAttribute
    {
        public override object GetDefaultMetadata(MemberInfo owner)
        {
            return null;
        }

        public override bool IsMetadataValid(object metadata)
        {
            return metadata == null;
        }
    }
}
