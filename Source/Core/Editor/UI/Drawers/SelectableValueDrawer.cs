using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.UI.SelectableValues;

namespace VRBuilder.Core.Editor.UI.Drawers
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
                MemberInfo firstMemberInfo = selectableValue.GetType().GetMember(nameof(selectableValue.FirstValue)).First();
                rect.height += DrawerLocator.GetDrawerForMember(firstMemberInfo, selectableValue).Draw(rect, selectableValue.FirstValue, (value) => ChangeValue(() => value, () => selectableValue.FirstValue, (newValue) =>
                {
                    selectableValue.FirstValue = (TFirst)newValue;
                    changeValueCallback(selectableValue);
                }), label).height;
            }
            else
            {
                MemberInfo secondMemberInfo = selectableValue.GetType().GetMember(nameof(selectableValue.SecondValue)).First();
                rect.height += DrawerLocator.GetDrawerForMember(secondMemberInfo, selectableValue).Draw(rect, selectableValue.SecondValue, (value) => ChangeValue(() => value, () => selectableValue.SecondValue, (newValue) =>
                {
                    selectableValue.SecondValue = (TSecond)newValue;
                    changeValueCallback(selectableValue);
                }), label).height;
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
