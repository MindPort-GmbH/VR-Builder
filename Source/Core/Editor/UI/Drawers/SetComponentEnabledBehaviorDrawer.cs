// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(SetComponentEnabledBehavior.EntityData))]
    public class SetComponentEnabledBehaviorDrawer : NameableDrawer
    {
        private const string noComponentSelected = "<none>";
        private const string unavailableSuffix = " (unavailable)";

        internal sealed class ComponentTypeSelection
        {
            public IReadOnlyList<string> Values { get; }
            public IReadOnlyList<string> Labels { get; }
            public int SelectedIndex { get; }
            public bool IsSelectedTypeUnavailable { get; }

            public ComponentTypeSelection(IReadOnlyList<string> values, IReadOnlyList<string> labels, int selectedIndex, bool isSelectedTypeUnavailable)
            {
                Values = values;
                Labels = labels;
                SelectedIndex = selectedIndex;
                IsSelectedTypeUnavailable = isSelectedTypeUnavailable;
            }
        }

        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect = base.Draw(rect, currentValue, changeValueCallback, label);
            float height = DrawLabel(rect, currentValue, changeValueCallback, label);
            height += EditorDrawingHelper.VerticalSpacing;
            Rect nextPosition = new Rect(rect.x, rect.y + height, rect.width, rect.height);

            SetComponentEnabledBehavior.EntityData data = currentValue as SetComponentEnabledBehavior.EntityData;

            nextPosition = DrawerLocator.GetDrawerForValue(data.TargetObjects, typeof(MultipleSceneObjectReference)).Draw(nextPosition, data.TargetObjects, changeValueCallback, "Objects");

            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;
            nextPosition.height = EditorDrawingHelper.SingleLineHeight;

            List<Component> components = new List<Component>();

            if (data.TargetObjects.IsEmpty() == false)
            {
                components = data.TargetObjects.Values
                    .SelectMany(sceneObject => sceneObject.SceneObject.GameObject.GetComponents<Component>())
                    .Where(CanBeDisabled)
                    .Where(component => component is ISceneObject == false && component is ISceneObjectProperty == false) // Make it impossible to use this behavior to disable VR Builder components
                    .ToList();
            }

            ComponentTypeSelection componentTypeSelection = BuildComponentTypeSelection(
                components.Select(component => component.GetType().Name),
                data.ComponentType);

            int currentComponent = componentTypeSelection.SelectedIndex;
            int newComponent = EditorGUI.Popup(nextPosition, "Component type", currentComponent, componentTypeSelection.Labels.ToArray());

            if (newComponent != currentComponent)
            {
                currentComponent = newComponent;

                if (currentComponent == 0)
                {
                    ChangeComponentType("", data, changeValueCallback);
                }
                else
                {
                    ChangeComponentType(componentTypeSelection.Values[currentComponent], data, changeValueCallback);
                }

                changeValueCallback(data);
            }

            height += EditorDrawingHelper.SingleLineHeight;

            if (componentTypeSelection.IsSelectedTypeUnavailable)
            {
                height += EditorDrawingHelper.VerticalSpacing;
                nextPosition.y = rect.y + height;
                nextPosition.height = EditorDrawingHelper.SingleLineHeight * 2;
                EditorGUI.HelpBox(
                    nextPosition,
                    $"Selected component type '{data.ComponentType}' is unavailable. The referenced scene may be unloaded or the component may have been removed.",
                    MessageType.Warning);
                height += nextPosition.height;
            }

            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            string revertState = data.SetEnabled ? "Disable" : "Enable";
            nextPosition = DrawerLocator.GetDrawerForValue(data.RevertOnDeactivation, typeof(bool)).Draw(nextPosition, data.RevertOnDeactivation, (value) => UpdateRevertOnDeactivate(value, data, changeValueCallback), $"{revertState} at end of step");

            height += EditorDrawingHelper.SingleLineHeight;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            rect.height = height;
            return rect;
        }

        internal static ComponentTypeSelection BuildComponentTypeSelection(IEnumerable<string> availableComponentTypes, string selectedComponentType)
        {
            List<string> values = new List<string> { string.Empty };
            values.AddRange(availableComponentTypes
                .Where(componentType => string.IsNullOrEmpty(componentType) == false)
                .Distinct());

            bool isSelectedTypeUnavailable = string.IsNullOrEmpty(selectedComponentType) == false && values.Contains(selectedComponentType) == false;

            if (isSelectedTypeUnavailable)
            {
                values.Insert(1, selectedComponentType);
            }

            List<string> labels = values
                .Select(componentType => string.IsNullOrEmpty(componentType)
                    ? noComponentSelected
                    : isSelectedTypeUnavailable && componentType == selectedComponentType
                        ? $"{componentType}{unavailableSuffix}"
                        : componentType)
                .ToList();

            int selectedIndex = string.IsNullOrEmpty(selectedComponentType) ? 0 : values.IndexOf(selectedComponentType);
            return new ComponentTypeSelection(values, labels, selectedIndex, isSelectedTypeUnavailable);
        }

        private bool CanBeDisabled(Component component)
        {
            return component.GetType().GetProperty("enabled") != null;
        }

        private void ChangeComponentType(string newValue, SetComponentEnabledBehavior.EntityData data, Action<object> changeValueCallback)
        {
            string oldValue = data.ComponentType;

            if (newValue != oldValue)
            {
                RevertableChangesHandler.Do(
                    new ProcessCommand(
                        () =>
                        {
                            data.ComponentType = newValue;
                            changeValueCallback(data);
                        },
                        () =>
                        {
                            data.ComponentType = oldValue;
                            changeValueCallback(data);
                        }));
            }
        }

        private void UpdateRevertOnDeactivate(object value, SetComponentEnabledBehavior.EntityData data, Action<object> changeValueCallback)
        {
            bool newValue = (bool)value;
            bool oldValue = data.RevertOnDeactivation;

            if (newValue != oldValue)
            {
                RevertableChangesHandler.Do(
                    new ProcessCommand(
                        () =>
                        {
                            data.RevertOnDeactivation = newValue;
                            changeValueCallback(data);
                        },
                        () =>
                        {
                            data.RevertOnDeactivation = oldValue;
                            changeValueCallback(data);
                        }));
            }
        }
    }
}
