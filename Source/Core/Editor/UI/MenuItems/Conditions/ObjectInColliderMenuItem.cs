using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.MenuItems.Conditions
{
    /// <inheritdoc />
    public class ObjectInColliderMenuItem : MenuItem<ICondition>
    {
        /// <inheritdoc />
        public override string DisplayedName { get; } = "Environment/Move Objects in Collider";

        /// <inheritdoc />
        public override ICondition GetNewItem()
        {
            return new ObjectInColliderCondition();
        }
    }
}
