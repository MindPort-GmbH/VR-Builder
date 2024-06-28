using UnityEngine;

namespace VRBuilder.Editor.UI.Drawers
{
    public struct DropDownElement<T>
    {
        public GUIContent Label { get; set; }
        public T Value { get; set; }

        public DropDownElement(T value, GUIContent label)
        {
            Value = value;
            Label = label;
        }

        public DropDownElement(T value, string label)
        {
            Value = value;
            Label = new GUIContent(label);
        }
    }
}
