using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Replaces an obsolete <see cref="SetObjectsWithTagEnabledBehavior"/> with a <see cref="SetObjectsEnabledBehavior"/>
    /// with the same configuration.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class SetObjectsWithTagEnabledBehaviorConverter : Converter<SetObjectsWithTagEnabledBehavior, SetObjectsEnabledBehavior>
    {
        /// <inheritdoc/>
        protected override SetObjectsEnabledBehavior PerformConversion(SetObjectsWithTagEnabledBehavior oldBehavior)
        {
#pragma warning restore CS0618 // Type or member is obsolete

            SetObjectsEnabledBehavior newBehavior = new SetObjectsEnabledBehavior();
            newBehavior.Data.SetEnabled = oldBehavior.Data.SetEnabled;
            if (oldBehavior.Data.TargetObjects.HasValue())
            {
                newBehavior.Data.TargetObjects = oldBehavior.Data.TargetObjects;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                newBehavior.Data.TargetObjects = new MultipleSceneObjectReference(oldBehavior.Data.Tag.Guid);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            newBehavior.Data.RevertOnDeactivation = oldBehavior.Data.RevertOnDeactivation;

            return newBehavior;
        }
    }
}
