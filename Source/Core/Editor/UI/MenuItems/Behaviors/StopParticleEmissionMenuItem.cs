using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Behaviors
{
    /// <inheritdoc />
    public class StopParticleEmissionMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Stop Particle Emission";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new ControlParticleEmissionBehavior(false);
        }
    }
}
