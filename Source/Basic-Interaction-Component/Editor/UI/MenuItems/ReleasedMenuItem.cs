using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.BasicInteraction.Editor.UI.MenuItems
{
    public class ReleasedMenuItem : MenuItem<ICondition>
    {
        public override string DisplayedName { get; } = "Interaction/Release Object";

        public override ICondition GetNewItem()
        {
            return new ReleasedCondition();
        }
    }
}
