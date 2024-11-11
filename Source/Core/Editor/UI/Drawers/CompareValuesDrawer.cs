using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.UI.SelectableValues;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Custom drawer for <see cref="CompareValuesCondition{T}"/>.
    /// </summary>    
    public abstract class CompareValuesDrawer<T> : NameableDrawer where T : IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Draws the dropdown for selecting the operator depending on the operands' type
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

            nextPosition = DrawerLocator.GetDrawerForValue(data.Left, typeof(ProcessVariableSelectableValue<T>)).Draw(nextPosition, data.Left, (value) => UpdateLeftOperand(value, data, changeValueCallback), "Left Operand");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            nextPosition = DrawOperatorDropdown(changeValueCallback, nextPosition, data);
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            nextPosition = DrawerLocator.GetDrawerForValue(data.Right, typeof(ProcessVariableSelectableValue<T>)).Draw(nextPosition, data.Right, (value) => UpdateRightOperand(value, data, changeValueCallback), "Right Operand");
            height += nextPosition.height;
            nextPosition.y = rect.y + height;

            rect.height = height;
            return rect;
        }

        private void UpdateLeftOperand(object value, CompareValuesCondition<T>.EntityData data, Action<object> changeValueCallback)
        {
            ProcessVariableSelectableValue<T> newOperand = (ProcessVariableSelectableValue<T>)value;

            if (newOperand != data.Left)
            {
                data.Left = newOperand;
                changeValueCallback(data);
            }
        }

        private void UpdateRightOperand(object value, CompareValuesCondition<T>.EntityData data, Action<object> changeValueCallback)
        {
            ProcessVariableSelectableValue<T> newOperand = (ProcessVariableSelectableValue<T>)value;

            if (newOperand != data.Right)
            {
                data.Right = newOperand;
                changeValueCallback(data);
            }
        }
    }
}
