// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit instantiator drawer for conditions.
    /// Creates a ToolbarMenu populated from EditorConfigurator.Instance.ConditionsMenuContent.
    /// </summary>
    [InstantiatorProcessElementDrawer(typeof(ICondition))]
    public class ConditionInstantiatorElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            VisualElement container = new VisualElement();

            ToolbarMenu addMenu = new ToolbarMenu { text = "Add Condition" };

            try
            {
                var menuContent = EditorConfigurator.Instance.ConditionsMenuContent;
                if (menuContent != null)
                {
                    foreach (MenuOption<ICondition> option in menuContent)
                    {
                        if (option is MenuItem<ICondition> menuItem)
                        {
                            string displayedName = menuItem.DisplayedName;
                            addMenu.menu.AppendAction(displayedName, _ =>
                            {
                                ICondition newItem = menuItem.GetNewItem();
                                ChangeValue(() => newItem, () => currentValue, changeValueCallback);
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                // EditorConfigurator may not be available
            }

            container.Add(addMenu);
            return container;
        }
    }
}
