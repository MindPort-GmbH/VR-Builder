using System;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
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

            ProcessVariableSelectableValue<T> left = new ProcessVariableSelectableValue<T>(data.LeftValue, data.LeftProperty, data.IsLeftConst);

            nextPosition = DrawerLocator.GetDrawerForValue(left, typeof(ProcessVariableSelectableValue<T>)).Draw(nextPosition, left, (value) => UpdateLeftOperand(value, data, changeValueCallback), "Left Operand");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            nextPosition = DrawOperatorDropdown(changeValueCallback, nextPosition, data);
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            ProcessVariableSelectableValue<T> right = new ProcessVariableSelectableValue<T>(data.RightValue, data.RightProperty, data.IsRightConst);

            nextPosition = DrawerLocator.GetDrawerForValue(right, typeof(ProcessVariableSelectableValue<T>)).Draw(nextPosition, right, (value) => UpdateRightOperand(value, data, changeValueCallback), "Right Operand");
            height += nextPosition.height;
            nextPosition.y = rect.y + height;

            rect.height = height;
            return rect;
        }

        private void UpdateLeftOperand(object value, CompareValuesCondition<T>.EntityData data, Action<object> changeValueCallback)
        {
            ProcessVariableSelectableValue<T> newOperand = (ProcessVariableSelectableValue<T>)value;
            ProcessVariableSelectableValue<T> oldOperand = new ProcessVariableSelectableValue<T>(data.LeftValue, data.LeftProperty, data.IsLeftConst);

            bool valueChanged = false;

            if (newOperand.SecondValue != oldOperand.SecondValue)
            {
                data.LeftProperty = newOperand.SecondValue;
                valueChanged = true;
            }

            if (newOperand.FirstValue != null && newOperand.FirstValue.Equals(oldOperand.FirstValue) == false)
            {
                data.LeftValue = newOperand.FirstValue;
                valueChanged = true;
            }

            if (newOperand.IsFirstValueSelected != oldOperand.IsFirstValueSelected)
            {
                data.IsLeftConst = newOperand.IsFirstValueSelected;
                valueChanged = true;
            }

            if (valueChanged)
            {
                changeValueCallback(data);
            }
        }

        private void UpdateRightOperand(object value, CompareValuesCondition<T>.EntityData data, Action<object> changeValueCallback)
        {
            ProcessVariableSelectableValue<T> newOperand = (ProcessVariableSelectableValue<T>)value;
            ProcessVariableSelectableValue<T> oldOperand = new ProcessVariableSelectableValue<T>(data.RightValue, data.RightProperty, data.IsRightConst);

            bool valueChanged = false;

            if (newOperand.SecondValue.Guids != oldOperand.SecondValue.Guids)
            {
                data.RightProperty = newOperand.SecondValue;
                valueChanged = true;
            }

            if (newOperand.FirstValue != null && newOperand.FirstValue.Equals(oldOperand.FirstValue) == false)
            {
                data.RightValue = newOperand.FirstValue;
                valueChanged = true;
            }

            if (newOperand.IsFirstValueSelected != oldOperand.IsFirstValueSelected)
            {
                data.IsRightConst = newOperand.IsFirstValueSelected;
                valueChanged = true;
            }

            if (valueChanged)
            {
                changeValueCallback(data);
            }
        }
    }
}
