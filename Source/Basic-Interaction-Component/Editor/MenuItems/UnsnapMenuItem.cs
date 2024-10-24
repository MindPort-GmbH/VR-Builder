using VRBuilder.BasicInteraction.Behaviors;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.BasicInteraction.Editor.UI.Behaviors
{
    /// <inheritdoc/>
    public class UnsnapMenuItem : MenuItem<IBehavior>
    {
        public override string DisplayedName { get; } = "Environment/Unsnap Object";

        public override IBehavior GetNewItem()
        {
            return new UnsnapBehavior();
        }
    }
}