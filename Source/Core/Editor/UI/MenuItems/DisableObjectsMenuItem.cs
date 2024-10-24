using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
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
