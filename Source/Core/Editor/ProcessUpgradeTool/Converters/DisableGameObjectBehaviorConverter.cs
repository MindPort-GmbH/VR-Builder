using System.Linq;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Replaces an obsolete <see cref="DisableGameObjectBehavior"/> with a <see cref="SetObjectsEnabledBehavior"/>
    /// with the same configuration.
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    public class DisableGameObjectBehaviorConverter : Converter<DisableGameObjectBehavior, SetObjectsEnabledBehavior>
    {
        /// <inheritdoc/>
        protected override SetObjectsEnabledBehavior PerformConversion(DisableGameObjectBehavior oldBehavior)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            SetObjectsEnabledBehavior newBehavior = new SetObjectsEnabledBehavior();
            newBehavior.Data.SetEnabled = false;
            newBehavior.Data.RevertOnDeactivation = false;

            if (string.IsNullOrEmpty(oldBehavior.Data.Target.UniqueName) == false)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ProcessSceneObject referencedObject = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().FirstOrDefault(sceneObject => sceneObject.UniqueName == oldBehavior.Data.Target.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

                if (referencedObject != null)
                {
                    newBehavior.Data.TargetObjects = new MultipleSceneObjectReference(referencedObject.Guid);
                }
            }

            return newBehavior;
        }
    }
}
