using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VRBuilder.Core.Editor.Tabs
{
    /// <summary>
    /// Tabs group which has the selected tab passed externally.
    /// </summary>
    [DataContract]
    internal class GlobalTabsGroup : ITabsGroup
    {
        public int Selected { get; set; }

        public IList<ITab> Tabs { get; }

        public GlobalTabsGroup(int selected, params ITab[] tabs)
        {
            Selected = selected;
            Tabs = tabs;
        }
    }
}
