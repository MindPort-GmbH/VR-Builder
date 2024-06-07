using System;
using System.Runtime.Serialization;
using VRBuilder.Core;

namespace VRBuilder.Editor.Tabs
{
    /// <summary>
    /// Tabs group which has the selected tab passed externally.
    /// </summary>
    [DataContract]
    internal class GlobalTabsGroup : TabsGroup
    {
        private int selected;

        public override int Selected
        {
            get
            {
                if (selected >= 0)
                {
                    return selected;
                }
                else
                {
                    var value = ParentMetadata.GetMetadata(GetType())[SelectedKey];
                    return Convert.ToInt32(value);
                }
            }

            set
            {
                selected = value;
                if (selected >= 0)
                {
                    ParentMetadata.SetMetadata(GetType(), SelectedKey, value);
                }
            }
        }

        public GlobalTabsGroup(int selected, Metadata parentMetadata, params ITab[] tabs) : base(parentMetadata, tabs)
        {
            Selected = selected;
        }
    }
}
