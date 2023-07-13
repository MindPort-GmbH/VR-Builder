using System;

namespace VRBuilder.Core.Attributes
{
    /// <summary>
    /// Tooltip of process entity's property or field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class DisplayTooltipAttribute : Attribute
    {
        /// <summary>
        /// Tooltip of the process entity's property or field.
        /// </summary>
        public string Tooltip { get; private set; }

        public DisplayTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}
