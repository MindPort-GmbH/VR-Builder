using System.Linq;
using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class SnappedObjectWithTagConditionConverter : Converter<SnappedObjectWithTagCondition, SnappedCondition>
    {
        protected override SnappedCondition PerformConversion(SnappedObjectWithTagCondition oldCondition)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            SnappedCondition newCondition = new SnappedCondition();

            if (oldCondition.Data.TargetObjects.HasValue())
            {
                newCondition.Data.TargetObjects = oldCondition.Data.TargetObjects;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                newCondition.Data.TargetObjects = new MultipleScenePropertyReference<ISnappableProperty>(oldCondition.Data.Tag.Guid);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (oldCondition.Data.TargetSnapZone.HasValue())
            {
                newCondition.Data.TargetSnapZone = oldCondition.Data.TargetSnapZone;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ProcessSceneObject referencedObject = SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().FirstOrDefault(sceneObject => sceneObject.UniqueName == oldCondition.Data.ZoneToSnapInto.UniqueName);
#pragma warning restore CS0618 // Type or member is obsolete

                if (referencedObject != null)
                {
                    newCondition.Data.TargetSnapZone = new SingleScenePropertyReference<ISnapZoneProperty>(referencedObject.Guid);
                }
            }

            return newCondition;
        }
    }
}