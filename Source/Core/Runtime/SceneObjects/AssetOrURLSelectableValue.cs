using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.SceneObjects
{
    public class AssetOrURLSelectableValue : SelectableValue<string, string>
    {
        public override string FirstValueLabel => "Video clip asset";

        public override string SecondValueLabel => "URL";

        [DataMember]
        [UsesSpecificProcessDrawer("AssetPathDrawer")]
        public override string FirstValue { get; set; }

        public AssetOrURLSelectableValue()
        {
            FirstValue = string.Empty;
            SecondValue = string.Empty;
        }
    }
}
