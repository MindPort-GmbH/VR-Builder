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
                    .SelectMany(sceneObject => sceneObject.GameObject.GetComponents<Component>())
                    .Where(CanBeDisabled)
                    .Where(component => component is ISceneObject == false && component is ISceneObjectProperty == false) // Make it impossible to use this behavior to disable VR Builder components
                    .ToList();
            }

            int currentComponent = 0;

            List<string> componentLabels = components.Select(c => c.GetType().Name).ToList();
            componentLabels.Insert(0, noComponentSelected);

            if (string.IsNullOrEmpty(data.ComponentType) == false)
            {
                if (componentLabels.Contains(data.ComponentType))
                {
                    currentComponent = componentLabels.IndexOf(componentLabels.First(l => l == data.ComponentType));
                }
                else
                {
                    currentComponent = 0;
                    ChangeComponentType("", data, changeValueCallback);
                }
            }

            int newComponent = EditorGUI.Popup(nextPosition, "Component type", currentComponent, componentLabels.ToArray());

            if (newComponent != currentComponent)
            {
                currentComponent = newComponent;

                if (currentComponent == 0)
                {
                    ChangeComponentType("", data, changeValueCallback);
                }
                else
                {
                    ChangeComponentType(componentLabels[currentComponent], data, changeValueCallback);
                }

                changeValueCallback(data);
            }

            height += EditorDrawingHelper.SingleLineHeight;
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
