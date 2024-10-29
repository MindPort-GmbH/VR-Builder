using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    public class ConfettiMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Guidance/Spawn Confetti";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new ConfettiBehavior();
        }
    }
}
