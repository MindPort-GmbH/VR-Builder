using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.BehaviorDrawers
{
    /// <summary>
    /// UIToolkit drawer for <see cref="SetComponentEnabledBehavior.EntityData"/>.
    /// The data members are tagged <c>[HideInProcessInspector]</c> because the legacy
    /// IMGUI drawer rendered them by hand — this class restores the equivalent layout
    /// (object drop slot, component-type popup, "Enable/Disable at end of step" toggle)
    /// for the UITK step inspector.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(SetComponentEnabledBehavior.EntityData))]
    internal class SetComponentEnabledBehaviorElementDrawer : ElementDrawer
    {
        private const string NoComponentSelected = "<none>";

        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("vrb-object");
            root.AddToClassList("vrb-set-component-enabled");

            if (value is not SetComponentEnabledBehavior.EntityData data)
            {
                root.Add(new Label("(invalid data)"));
                return root;
            }

            root.Add(BuildTargetObjectsField(data, changeCallback));
            root.Add(BuildComponentTypeField(data, changeCallback));
            root.Add(BuildRevertToggle(data, changeCallback));

            return root;
        }

        private VisualElement BuildTargetObjectsField(SetComponentEnabledBehavior.EntityData data, Action<object> changeCallback)
        {
            IElementDrawer referenceDrawer = ElementDrawerLocator.GetDrawerForValue(
                data.TargetObjects, typeof(MultipleSceneObjectReference));

            if (referenceDrawer == null)
            {
                return new Label("(no drawer for MultipleSceneObjectReference)");
            }

            return referenceDrawer.CreateElement(
                data.TargetObjects,
                newValue =>
                {
                    data.TargetObjects = (MultipleSceneObjectReference)newValue;
                    changeCallback(data);
                },
                new GUIContent("Objects"));
        }

        private VisualElement BuildComponentTypeField(SetComponentEnabledBehavior.EntityData data, Action<object> changeCallback)
        {
            List<string> options = BuildComponentOptions(data);
            string current = ResolveCurrentSelection(data, options, changeCallback);

            DropdownField field = new DropdownField("Component type", options, current)
            {
                tooltip = "Component type to enable or disable on the referenced objects."
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--component-type");

            field.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (evt.newValue == evt.previousValue) return;

                string oldValue = data.ComponentType ?? string.Empty;
                string newValue = evt.newValue == NoComponentSelected ? string.Empty : evt.newValue;

                if (newValue == oldValue) return;

                ChangeValue(
                    getNewValueCallback: () => newValue,
                    getOldValueCallback: () => oldValue,
                    assignValueCallback: applied =>
                    {
                        data.ComponentType = (string)applied;
                        changeCallback(data);
                    });
            });

            return field;
        }

        private VisualElement BuildRevertToggle(SetComponentEnabledBehavior.EntityData data, Action<object> changeCallback)
        {
            string label = data.SetEnabled ? "Disable at end of step" : "Enable at end of step";

            Toggle toggle = new Toggle(label)
            {
                value = data.RevertOnDeactivation,
                tooltip = "If enabled, the component reverts to its original state when the step ends."
            };
            toggle.AddToClassList("vrb-field");
            toggle.AddToClassList("vrb-field--revert-on-deactivation");

            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue == evt.previousValue) return;

                bool oldValue = data.RevertOnDeactivation;
                bool newValue = evt.newValue;

                ChangeValue(
                    getNewValueCallback: () => newValue,
                    getOldValueCallback: () => oldValue,
                    assignValueCallback: applied =>
                    {
                        data.RevertOnDeactivation = (bool)applied;
                        changeCallback(data);
                    });
            });

            return toggle;
        }

        private static List<string> BuildComponentOptions(SetComponentEnabledBehavior.EntityData data)
        {
            List<string> options = new List<string> { NoComponentSelected };

            if (data.TargetObjects == null || data.TargetObjects.IsEmpty())
            {
                return options;
            }

            // VR Builder components themselves are excluded so the behavior can't be used to
            // turn off the very plumbing it relies on (ISceneObject / ISceneObjectProperty).
            IEnumerable<string> componentNames = data.TargetObjects.Values
                .Where(sceneObject => sceneObject?.GameObject != null)
                .SelectMany(sceneObject => sceneObject.GameObject.GetComponents<Component>())
                .Where(component => component != null && CanBeDisabled(component))
                .Where(component => component is ISceneObject == false && component is ISceneObjectProperty == false)
                .Select(component => component.GetType().Name)
                .Distinct();

            options.AddRange(componentNames);
            return options;
        }

        private static string ResolveCurrentSelection(
            SetComponentEnabledBehavior.EntityData data,
            List<string> options,
            Action<object> changeCallback)
        {
            if (string.IsNullOrEmpty(data.ComponentType))
            {
                return NoComponentSelected;
            }

            if (options.Contains(data.ComponentType))
            {
                return data.ComponentType;
            }

            // The previously selected component is no longer present on any referenced object —
            // clear the stored value so the popup and the persisted data agree.
            string stale = data.ComponentType;
            data.ComponentType = string.Empty;
            changeCallback(data);
            UnityEngine.Debug.LogWarning($"Component '{stale}' is no longer available on the referenced objects — selection cleared.");
            return NoComponentSelected;
        }

        private static bool CanBeDisabled(Component component)
        {
            return component.GetType().GetProperty("enabled") != null;
        }
    }
}
