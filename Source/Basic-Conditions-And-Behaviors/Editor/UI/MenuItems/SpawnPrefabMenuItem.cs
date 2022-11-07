using VRBuilder.Animations.Behaviors;
using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
{
    public class SpawnPrefabMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Spawn Prefab";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new SpawnPrefabBehavior();
        }
    }
}
