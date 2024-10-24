using VRBuilder.Core.Behaviors;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
{
    /// <inheritdoc />
    public class StartParticleEmissionMenuItem : MenuItem<IBehavior>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Start Particle Emission";

        /// <inheritdoc />
        public override IBehavior GetNewItem()
        {
            return new ControlParticleEmissionBehavior(true);
        }
    }
}
