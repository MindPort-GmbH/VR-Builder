using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Conditions
{
    /// <inheritdoc />
    public class TimeoutMenuItem : MenuItem<ICondition>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Utility/Timeout";

        /// <inheritdoc />
        public override ICondition GetNewItem()
        {
            return new TimeoutCondition();
        }
    }
}
