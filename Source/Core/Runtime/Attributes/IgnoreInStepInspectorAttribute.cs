using System;

namespace VRBuilder.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreInStepInspectorAttribute : Attribute
    {
    }
}
