using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Conditions;
using VRBuilder.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Editor.UI.Behaviors
{
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder-tutorials/creating-custom-conditions")]
    public class CollectedConditionMenuItem : MenuItem<ICondition>
    {
        /// <inheritdoc />
        public override string DisplayedName { get => "Environment/New Object in Collider"; }
        /// <inheritdoc />
        /// 
        public override ICondition GetNewItem()
        {
            return new CollectedCondition();
        }
    }
}