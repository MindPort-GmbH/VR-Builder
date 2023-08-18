using System;
using UnityEngine;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Editor.UI.Drawers
{
    public class SelectableValueDrawer<TFirst, TSecond> : AbstractDrawer
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
            if (GUILayout.Toggle(!selectableValue.IsFirstValueSelected, selectableValue.FirstValueLabel, BuilderEditorStyles.RadioButton))
            {
                selectableValue.IsFirstValueSelected = false;
                changeValueCallback(selectableValue);
            }
            GUILayout.EndHorizontal();

            if (selectableValue.IsFirstValueSelected) 
            {
                DrawerLocator.GetDrawerForValue(selectableValue.FirstValue, typeof(TFirst)).Draw(rect, selectableValue.FirstValue, (value) => ChangeValue(() => value, () => selectableValue.FirstValue, (newValue) => selectableValue.FirstValue = (TFirst)newValue), "");
            }
            else
            {
                DrawerLocator.GetDrawerForValue(selectableValue.SecondValue, typeof(TSecond)).Draw(rect, selectableValue.SecondValue, (value) => ChangeValue(() => value, () => selectableValue.SecondValue, (newValue) => selectableValue.SecondValue = (TSecond)newValue), "");
            }

            GUILayout.EndArea();
            return rect;
        }
    }
}
