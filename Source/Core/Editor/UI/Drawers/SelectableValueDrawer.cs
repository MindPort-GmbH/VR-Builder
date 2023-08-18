using System;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    /// <summary>
    /// Template drawer for selectable values. A concrete implementation of this drawer is required for each use case.
    /// </summary>
    public abstract class SelectableValueDrawer<TFirst, TSecond> : AbstractDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return rect;
            }

            SelectableValue<TFirst, TSecond> selectableValue = (SelectableValue<TFirst, TSecond>)currentValue;
            Type firstType = selectableValue.FirstValue.GetType();
            Type secondType = selectableValue.SecondValue.GetType();

            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(selectableValue.IsFirstValueSelected, selectableValue.FirstValueLabel, BuilderEditorStyles.RadioButton))
            {
                selectableValue.IsFirstValueSelected = true;
                changeValueCallback(selectableValue);
            }
            if (GUILayout.Toggle(!selectableValue.IsFirstValueSelected, selectableValue.SecondValueLabel, BuilderEditorStyles.RadioButton))
            {
                selectableValue.IsFirstValueSelected = false;
                changeValueCallback(selectableValue);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            rect = AddNewRectLine(ref rect);

            if (selectableValue.IsFirstValueSelected) 
            {
                rect.height += DrawerLocator.GetDrawerForValue(selectableValue.FirstValue, firstType).Draw(rect, selectableValue.FirstValue, (value) => ChangeValue(() => value, () => selectableValue.FirstValue, (newValue) => selectableValue.FirstValue = (TFirst)newValue), label).height;
            }
            else
            {
                rect.height += DrawerLocator.GetDrawerForValue(selectableValue.SecondValue, secondType).Draw(rect, selectableValue.SecondValue, (value) => ChangeValue(() => value, () => selectableValue.SecondValue, (newValue) => selectableValue.SecondValue = (TSecond)newValue), label).height;
            }

            rect.height += EditorDrawingHelper.VerticalSpacing;

            return rect;
        }

        protected Rect AddNewRectLine(ref Rect currentRect)
        {
            Rect newRectLine = currentRect;
            newRectLine.height = EditorDrawingHelper.SingleLineHeight;
            newRectLine.y += currentRect.height + EditorDrawingHelper.VerticalSpacing;

            currentRect.height += EditorDrawingHelper.SingleLineHeight + EditorDrawingHelper.VerticalSpacing;
            return newRectLine;
        }
    }
}
