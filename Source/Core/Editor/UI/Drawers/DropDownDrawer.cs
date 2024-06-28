using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRBuilder.Editor.UI.Drawers
{
    public abstract class DropDownDrawer<T> : AbstractDrawer where T : IEquatable<T>
    {
        protected abstract IList<DropDownElement<T>> PossibleOptions { get; }

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect.height = EditorDrawingHelper.SingleLineHeight;

            T oldValue = (T)currentValue;

            int oldIndex = PossibleOptions.IndexOf(PossibleOptions.FirstOrDefault(item => item.Value.Equals(oldValue)));

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            int newIndex = EditorGUI.Popup(rect, label, oldIndex, PossibleOptions.Select(item => item.Label).ToArray());

            if (PossibleOptions[newIndex].Value.Equals(oldValue) == false)
            {
                ChangeValue(() => PossibleOptions[newIndex].Value, () => oldValue, changeValueCallback);
            }

            return rect;
        }
    }
}
