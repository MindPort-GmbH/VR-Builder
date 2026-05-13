// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.UI.SelectableValues;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Generic UITK port of the legacy <c>SelectableValueDrawer&lt;TFirst,TSecond&gt;</c>.
    /// Renders two RadioButtons (or Toggles) for the two value-source options, then the active
    /// member's drawer below them.
    /// </summary>
    /// <remarks>
    /// Concrete subclasses register themselves for a specific <see cref="SelectableValue{TFirst, TSecond}"/>
    /// instantiation via <see cref="DefaultProcessElementDrawerAttribute"/>.
    /// </remarks>
    public abstract class SelectableValueElementDrawer<TFirst, TSecond> : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            if (RuntimeConfigurator.Exists == false)
            {
                return new Label("Selectable values require a Process Controller in the scene.");
            }

            if (value is not SelectableValue<TFirst, TSecond> selectable)
            {
                return new Label("(invalid selectable value)");
            }

            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-selectable");

            VisualElement row = new VisualElement();
            row.AddToClassList("vrb-selectable__choices");
            row.style.flexDirection = FlexDirection.Row;

            RadioButton firstButton = new RadioButton(selectable.FirstValueLabel)
            {
                value = selectable.IsFirstValueSelected,
                tooltip = selectable.FirstValueLabel
            };
            firstButton.AddToClassList("vrb-selectable__choice");
            row.Add(firstButton);

            RadioButton secondButton = new RadioButton(selectable.SecondValueLabel)
            {
                value = !selectable.IsFirstValueSelected,
                tooltip = selectable.SecondValueLabel
            };
            secondButton.AddToClassList("vrb-selectable__choice");
            row.Add(secondButton);

            VisualElement valueContainer = new VisualElement();
            valueContainer.AddToClassList("vrb-selectable__value");

            firstButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue && selectable.IsFirstValueSelected == false)
                {
                    bool wasFirst = selectable.IsFirstValueSelected;
                    ChangeValue(
                        getNewValueCallback: () => { selectable.IsFirstValueSelected = true; return selectable; },
                        getOldValueCallback: () => { selectable.IsFirstValueSelected = wasFirst; return selectable; },
                        assignValueCallback: changeCallback);
                    RebuildValueArea(valueContainer, selectable, changeCallback, label);
                }
            });

            secondButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue && selectable.IsFirstValueSelected)
                {
                    bool wasFirst = selectable.IsFirstValueSelected;
                    ChangeValue(
                        getNewValueCallback: () => { selectable.IsFirstValueSelected = false; return selectable; },
                        getOldValueCallback: () => { selectable.IsFirstValueSelected = wasFirst; return selectable; },
                        assignValueCallback: changeCallback);
                    RebuildValueArea(valueContainer, selectable, changeCallback, label);
                }
            });

            root.Add(row);
            root.Add(valueContainer);
            RebuildValueArea(valueContainer, selectable, changeCallback, label);

            return root;
        }

        private void RebuildValueArea(VisualElement container, SelectableValue<TFirst, TSecond> selectable,
            Action<object> changeCallback, GUIContent label)
        {
            container.Clear();

            if (selectable.IsFirstValueSelected)
            {
                MemberInfo memberInfo = selectable.GetType().GetMember(nameof(selectable.FirstValue)).First();
                IElementDrawer firstDrawer = ElementDrawerLocator.GetDrawerForMember(memberInfo, selectable);
                if (firstDrawer == null) return;

                container.Add(firstDrawer.CreateElement(
                    selectable.FirstValue,
                    newValue =>
                    {
                        TFirst oldFirst = selectable.FirstValue;
                        TFirst newFirst = (TFirst)newValue;
                        ChangeValue(
                            getNewValueCallback: () => { selectable.FirstValue = newFirst; return selectable; },
                            getOldValueCallback: () => { selectable.FirstValue = oldFirst; return selectable; },
                            assignValueCallback: changeCallback);
                    },
                    label));
            }
            else
            {
                MemberInfo memberInfo = selectable.GetType().GetMember(nameof(selectable.SecondValue)).First();
                IElementDrawer secondDrawer = ElementDrawerLocator.GetDrawerForMember(memberInfo, selectable);
                if (secondDrawer == null) return;

                container.Add(secondDrawer.CreateElement(
                    selectable.SecondValue,
                    newValue =>
                    {
                        TSecond oldSecond = selectable.SecondValue;
                        TSecond newSecond = (TSecond)newValue;
                        ChangeValue(
                            getNewValueCallback: () => { selectable.SecondValue = newSecond; return selectable; },
                            getOldValueCallback: () => { selectable.SecondValue = oldSecond; return selectable; },
                            assignValueCallback: changeCallback);
                    },
                    label));
            }
        }
    }
}
