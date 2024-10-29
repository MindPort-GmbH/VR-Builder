using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class SetParentMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Utility/Set Parent";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new SetParentBehavior();
        }
    }
}
