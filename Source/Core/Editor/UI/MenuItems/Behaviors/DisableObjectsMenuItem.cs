using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class DisableObjectsMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Disable Objects";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new SetObjectsEnabledBehavior(false);
        }
    }
}
