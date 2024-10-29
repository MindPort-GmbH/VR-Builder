using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class EnableComponentMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Enable Components";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new SetComponentEnabledBehavior(true);
        }
    }
}
