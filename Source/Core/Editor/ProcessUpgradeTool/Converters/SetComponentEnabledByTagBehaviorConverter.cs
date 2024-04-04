using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Replaces an obsolete <see cref="SetComponentEnabledByTagBehavior"/> with a <see cref="SetComponentEnabledBehavior"/>
    /// with the same configuration.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class SetComponentEnabledByTagBehaviorConverter : Converter<SetComponentEnabledByTagBehavior, SetComponentEnabledBehavior>
    {
        /// <inheritdoc/>
        protected override SetComponentEnabledBehavior PerformConversion(SetComponentEnabledByTagBehavior oldBehavior)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            SetComponentEnabledBehavior newBehavior = new SetComponentEnabledBehavior();

            if (oldBehavior.Data.TargetObjects.HasValue())
            {
                newBehavior.Data.TargetObjects = oldBehavior.Data.TargetObjects;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                newBehavior.Data.TargetObjects = new MultipleSceneObjectReference(oldBehavior.Data.TargetTag.Guid);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            newBehavior.Data.SetEnabled = oldBehavior.Data.SetEnabled;
            newBehavior.Data.RevertOnDeactivation = oldBehavior.Data.RevertOnDeactivation;
            newBehavior.Data.ComponentType = oldBehavior.Data.ComponentType;

            return newBehavior;
        }
    }
}
