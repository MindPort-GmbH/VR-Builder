using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Conditions
{
    /// <inheritdoc />
    public class ObjectInRangeMenuItem : MenuItem<ICondition>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Object Nearby";

        /// <inheritdoc />
        public override ICondition GetNewItem()
        {
            return new ObjectInRangeCondition();
        }
    }
}
