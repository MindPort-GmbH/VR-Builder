using VRBuilder.BasicInteraction.Conditions;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.BasicInteraction.Editor.UI.MenuItems
{
    public class UsedMenuItem : MenuItem<ICondition>
    {
        public override string DisplayedName { get; } = "Interaction/Use Object";

        public override ICondition GetNewItem()
        {
            return new UsedCondition();
        }
    }
}