using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Conditions
{
    /// <inheritdoc />
    public class GoToChapterMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Utility/Go to Chapter";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new GoToChapterBehavior();
        }
    }
}
