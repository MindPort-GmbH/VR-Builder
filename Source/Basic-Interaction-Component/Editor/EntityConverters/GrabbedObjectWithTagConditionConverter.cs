using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.ProcessUpdater
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class GrabbedObjectWithTagConditionConverter : EntityConverter<GrabbedObjectWithTagCondition, GrabbedCondition>
    {
        protected override GrabbedCondition PerformConversion(GrabbedObjectWithTagCondition oldCondition)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            GrabbedCondition newCondition = new GrabbedCondition();

            if (oldCondition.Data.Targets.HasValue())
            {
                newCondition.Data.Targets = oldCondition.Data.Targets;
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                newCondition.Data.Targets = new MultipleScenePropertyReference<IGrabbableProperty>(oldCondition.Data.Tag.Guid);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            return newCondition;
        }
    }
}