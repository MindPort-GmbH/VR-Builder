using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Generic drawer for dropdowns. Implement by providing your possible options.
    /// </summary>
    /// <remarks>
    /// In case of a null or invalid value, the drawer will automatically select the first
    /// available value. You can create a null entry for the first value if you want it
    /// to default to null.
    /// </remarks>
    /// <typeparam name="T">Type of value provided by the dropdown.</typeparam>
    public abstract class DropdownDrawer<T> : AbstractDrawer where T : IEquatable<T>
    {
        /// <summary>
        /// List of elements displayed in the dropdown.
        /// </summary>
        protected abstract IList<DropDownElement<T>> PossibleOptions { get; }

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect.height = EditorDrawingHelper.SingleLineHeight;

            if (PossibleOptions.Count == 0)
            {
                EditorGUI.LabelField(rect, "No values can be selected.");
                return rect;
            }

            T oldValue = (T)currentValue;

            int oldIndex = PossibleOptions.IndexOf(PossibleOptions.FirstOrDefault(item => item.Value != null && item.Value.Equals(oldValue)));

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            int newIndex = EditorGUI.Popup(rect, label, oldIndex, PossibleOptions.Select(item => item.Label).ToArray());

            if (PossibleOptions[newIndex].Value == null)
            {
                if (oldValue != null)
                {
                    ChangeValue(() => PossibleOptions[newIndex].Value, () => oldValue, changeValueCallback);
                }
            }
            else if (PossibleOptions[newIndex].Value.Equals(oldValue) == false)
            {
                ChangeValue(() => PossibleOptions[newIndex].Value, () => oldValue, changeValueCallback);
            }

            return rect;
        }
    }
}
