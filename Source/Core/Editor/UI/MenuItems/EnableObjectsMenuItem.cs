using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
{
    /// <inheritdoc />
    public class EnableObjectsMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Enable Objects";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new SetObjectsEnabledBehavior(true);
        }
    }
}
