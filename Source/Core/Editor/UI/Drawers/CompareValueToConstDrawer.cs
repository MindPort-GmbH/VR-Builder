using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    /// <summary>
    /// Custom drawer for <see cref="CompareValuesCondition{T}"/>. This drawer omits the reference/const selectors in order
    /// to improve performance and optimize layout.
    /// </summary>
    public abstract class CompareValueToConstDrawer<T> : NameableDrawer where T : IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Draws the dropdown for selecting the operator depending on the operands' type.
        /// </summary>
        protected abstract Rect DrawOperatorDropdown(Action<object> changeValueCallback, Rect nextPosition, CompareValuesCondition<T>.EntityData data);

        /// <inheritdoc/>
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect = base.Draw(rect, currentValue, changeValueCallback, label);
            float height = rect.height;
            height += EditorDrawingHelper.VerticalSpacing;

            Rect nextPosition = new Rect(rect.x, rect.y + height, rect.width, rect.height);

            CompareValuesCondition<T>.EntityData data = currentValue as CompareValuesCondition<T>.EntityData;

            nextPosition = DrawerLocator.GetDrawerForValue(data.LeftProperty, typeof(SingleScenePropertyReference<IDataProperty<T>>)).Draw(nextPosition, data.LeftProperty, (value) => { data.LeftProperty = (SingleScenePropertyReference<IDataProperty<T>>)value; }, "Left Operand");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            nextPosition = DrawOperatorDropdown(changeValueCallback, nextPosition, data);
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            nextPosition = DrawerLocator.GetDrawerForValue(data.RightValue, typeof(T)).Draw(nextPosition, data.RightValue, (value) => data.RightValue = (T)value, "Right Operand");
            height += nextPosition.height;
            nextPosition.y = rect.y + height;

            if (data.IsLeftConst || data.IsRightConst == false)
            {
                data.IsLeftConst = false;
                data.IsRightConst = true;
                changeValueCallback(data);
            }

            rect.height = height;
            return rect;
        }
    }
}
