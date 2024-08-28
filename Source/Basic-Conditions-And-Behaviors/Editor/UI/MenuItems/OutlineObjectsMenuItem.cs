using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
{
    /// <inheritdoc />
    public class OutlineObjectsMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Guidance/Outline Objects";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new OutlineObjectsBehavior();
        }
    }
}
