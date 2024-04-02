using System.Linq;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class EnableGameObjectBehaviorConverter : Converter<EnableGameObjectBehavior, SetObjectsEnabledBehavior>
    {
        protected override SetObjectsEnabledBehavior PerformConversion(EnableGameObjectBehavior oldBehavior)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            SetObjectsEnabledBehavior newBehavior = new SetObjectsEnabledBehavior();
            newBehavior.Data.SetEnabled = true;
            newBehavior.Data.RevertOnDeactivation = oldBehavior.Data.DisableOnDeactivating;

#pragma warning disable CS0618 // Type or member is obsolete
            ProcessSceneObject referencedObject = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().FirstOrDefault(sceneObject => sceneObject.UniqueName == oldBehavior.Data.Target.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

            if (referencedObject != null)
            {
                newBehavior.Data.TargetObjects = new MultipleSceneObjectReference(referencedObject.Guid);
            }

            return newBehavior;
        }
    }
}
