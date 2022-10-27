using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.Core.Conditions;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.BasicInteraction.UI.Conditions
{
    public class GrabbedByTagMenuItem : MenuItem<ICondition>
    {
        public override string DisplayedName { get; } = "Interaction/Grab Object with Tag";

        public override ICondition GetNewItem()
        {
            return new GrabbedObjectWithTagCondition();
        }
    }
}