using System;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// An element in a dropdown.
    /// </summary>
    /// <typeparam name="T">Type of value returned by the dropdown.</typeparam>
    public struct DropDownElement<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Display name of the element.
        /// </summary>
        public GUIContent Label { get; set; }

        /// <summary>
        /// Selectable value.
        /// </summary>
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

        public DropDownElement(T value)
        {
            Value = value;
            Label = new GUIContent(value.ToString());
        }
    }
}
