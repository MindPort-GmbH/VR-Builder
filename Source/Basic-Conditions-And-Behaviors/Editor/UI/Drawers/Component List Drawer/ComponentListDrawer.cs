using System;
using System.Linq;
using UnityEngine;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Editor.UI;
using VRBuilder.Editor.UI.Drawers;

namespace VRBuilder.Editor.Core.UI.Drawers
{
    [DefaultProcessDrawer(typeof(EnableComponentBehavior.EntityData))]
    public class ComponentListDrawer : NameableDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            rect = base.Draw(rect, currentValue, changeValueCallback, label);

            float height = DrawLabel(rect, currentValue, changeValueCallback, label);

            height += EditorDrawingHelper.VerticalSpacing;

            Rect nextPosition = new Rect(rect.x, rect.y + height, rect.width, rect.height);

            EnableComponentBehavior.EntityData data = currentValue as EnableComponentBehavior.EntityData;            

            nextPosition = DrawerLocator.GetDrawerForValue(data.Target, typeof(SceneObjectReference)).Draw(nextPosition, data.Target, (value) => UpdateTargetObject(value, data, changeValueCallback), "Object");
            height += nextPosition.height;
            height += EditorDrawingHelper.VerticalSpacing;
            nextPosition.y = rect.y + height;

            if (RuntimeConfigurator.Configuration.SceneObjectRegistry.ContainsName(data.Target.UniqueName) && data.Target.Value != null)
            {
                Component[] components = data.Target.Value.GameObject.GetComponents<Component>().Where(CanBeDisabled).ToArray();

                for (int i = 0; i < components.Length; ++i)
                {
                    bool newValue = data.DisabledComponents.Contains(i) == false;
                    nextPosition = DrawerLocator.GetDrawerForValue(newValue, typeof(bool)).Draw(nextPosition, newValue, (value) => UpdateComponentEnabled(i, value, data, changeValueCallback), components[i].GetType().Name);
                    height += nextPosition.height;
                    height += EditorDrawingHelper.VerticalSpacing;
                    nextPosition.y = rect.y + height;
                }
            }

            rect.height = height;
            return rect;
        }

        private void UpdateComponentEnabled(int index, object value, EnableComponentBehavior.EntityData data, Action<object> changeValueCallback)
        {
            bool enabled = (bool)value;

            if(enabled)
            {
                data.DisabledComponents.Remove(index);
                changeValueCallback(data);
            }
            else
            {
                data.DisabledComponents.Add(index);
                changeValueCallback(data);
            }
        }

        private bool CanBeDisabled(Component component)
        {
            return component.GetType().GetProperty("enabled") != null;
        }

        private void UpdateTargetObject(object value, EnableComponentBehavior.EntityData data, Action<object> changeValueCallback)
        {
            SceneObjectReference newTarget = (SceneObjectReference)value;
            SceneObjectReference oldTarget = data.Target;

            if (newTarget != oldTarget)
            {
                data.Target = newTarget;
                changeValueCallback(data);
            }
        }


    }
}