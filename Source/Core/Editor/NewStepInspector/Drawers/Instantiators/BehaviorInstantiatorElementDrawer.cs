// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.Configuration;
using VRBuilder.Core.Editor.UI.StepInspector.Menu;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit instantiator drawer for behaviors.
    /// Creates a ToolbarMenu populated from EditorConfigurator.Instance.BehaviorsMenuContent.
    /// </summary>
    [InstantiatorProcessElementDrawer(typeof(IBehavior))]
    public class BehaviorInstantiatorElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            VisualElement container = new VisualElement();

            ToolbarMenu addMenu = new ToolbarMenu { text = "Add Behavior" };

            try
            {
                var menuContent = EditorConfigurator.Instance.BehaviorsMenuContent;
                if (menuContent != null)
                {
                    foreach (MenuOption<IBehavior> option in menuContent)
                    {
                        if (option is MenuItem<IBehavior> menuItem)
                        {
                            string displayedName = menuItem.DisplayedName;
                            addMenu.menu.AppendAction(displayedName, _ =>
                            {
                                IBehavior newItem = menuItem.GetNewItem();
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
