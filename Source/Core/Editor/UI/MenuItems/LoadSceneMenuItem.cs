using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
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
