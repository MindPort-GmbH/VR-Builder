using System.Collections.Generic;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SceneDropDownDrawer : DropDownDrawer<string>
    {
        protected override IList<DropDownElement<string>> PossibleOptions => options;

        private List<DropDownElement<string>> options = new List<DropDownElement<string>>()
        {
            new DropDownElement<string>(null, "<null>"),
            new DropDownElement<string>("test", "Test1"),
            new DropDownElement<string>("blah", "Test2"),
            new DropDownElement<string>("potato", "Potato")
        };
    }
}
