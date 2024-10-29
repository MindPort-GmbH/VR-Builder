using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class BehaviorSequenceMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Utility/Behaviors Sequence";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new BehaviorSequence();
        }
    }
}
