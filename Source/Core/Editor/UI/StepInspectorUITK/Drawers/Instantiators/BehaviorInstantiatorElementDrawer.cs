using System;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.Configuration;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Instantiators
{
    /// <summary>
    /// "+ Add Behavior" UI used by <see cref="MetadataWrapperElementDrawer"/> when an
    /// <c>[ExtendableList]</c> field contains <see cref="IBehavior"/> entries
    /// (e.g. <c>BehaviorSequence.EntityData.Behaviors</c>).
    /// </summary>
    [InstantiatorProcessElementDrawer(typeof(IBehavior))]
    internal class BehaviorInstantiatorElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            Button button = new Button(() =>
            {
                AddMenuHelper.ShowMenu<IBehavior>(
                    EditorConfigurator.Instance.BehaviorsMenuContent,
                    newBehavior =>
                    {
                        if (newBehavior == null) return;
                        changeCallback(newBehavior);
                    });
            })
            {
                text = "+ Add Behavior",
                tooltip = Tooltips.AddBehavior
            };
            button.AddToClassList("vrb-add-button");
            button.AddToClassList("vrb-add-button--behavior");
            return button;
        }
    }
}
