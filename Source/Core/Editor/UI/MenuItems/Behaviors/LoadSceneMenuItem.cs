using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class LoadSceneMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Utility/Load Scene";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new LoadSceneBehavior();
        }
    }
}
