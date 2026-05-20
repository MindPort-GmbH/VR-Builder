using System;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Editor.Configuration;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Instantiators
{
    /// <summary>
    /// "+ Add Condition" UI used by <see cref="MetadataWrapperElementDrawer"/> when an
    /// <c>[ExtendableList]</c> field contains <see cref="ICondition"/> entries.
    /// </summary>
    [InstantiatorProcessElementDrawer(typeof(ICondition))]
    internal class ConditionInstantiatorElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            Button button = new Button(() =>
            {
                AddMenuHelper.ShowMenu<ICondition>(
                    EditorConfigurator.Instance.ConditionsMenuContent,
                    newCondition =>
                    {
                        if (newCondition == null) return;
                        changeCallback(newCondition);
                    });
            })
            {
                text = "+ Add Condition",
                tooltip = Tooltips.AddCondition
            };
            button.AddToClassList("vrb-add-button");
            button.AddToClassList("vrb-add-button--condition");
            return button;
        }
    }
}
