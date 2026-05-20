using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Editor.UI.Drawers;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Base class for UIToolkit element drawers. Mirrors
    /// <see cref="VRBuilder.Core.Editor.UI.Drawers.AbstractDrawer"/> on the IMGUI side.
    /// </summary>
    public abstract class ElementDrawer : IElementDrawer
    {
        public abstract VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label);

        public virtual GUIContent GetLabel(MemberInfo memberInfo, object memberOwner)
        {
            Type memberType = MemberAccessCache.GetDeclaredType(memberInfo);
            object value = MemberAccessCache.GetValue(memberOwner, memberInfo);

            if (value != null)
            {
                memberType = value.GetType();
            }

            GUIContent valueLabel = GetLabel(value, memberType);

            DisplayNameAttribute displayNameAttribute =
                memberInfo.GetAttributes<DisplayNameAttribute>(true).FirstOrDefault();
            DisplayTooltipAttribute displayTooltipAttribute =
                memberInfo.GetAttributes<DisplayTooltipAttribute>(true).FirstOrDefault();

            if (displayNameAttribute?.Name != null)
            {
                valueLabel.text = displayNameAttribute.Name;
            }

            if (displayTooltipAttribute?.Tooltip != null)
            {
                valueLabel.tooltip = displayTooltipAttribute.Tooltip;
            }

            if (string.IsNullOrEmpty(valueLabel.text) && valueLabel.image == null)
            {
                valueLabel.text = memberInfo.Name;
            }

            return valueLabel;
        }

        public virtual GUIContent GetLabel(object value, Type declaredType)
        {
            INamedData nameable = value as INamedData;

            if (nameable == null || string.IsNullOrEmpty(nameable.Name))
            {
                return new GUIContent(string.Empty);
            }

            return new GUIContent(nameable.Name);
        }

        /// <summary>
        /// Routes a value change through <see cref="RevertableChangesHandler"/>
        /// so the edit shows up on Unity's undo stack.
        /// </summary>
        protected void ChangeValue(
            Func<object> getNewValueCallback,
            Func<object> getOldValueCallback,
            Action<object> assignValueCallback)
        {
            Action doCallback = () => assignValueCallback(getNewValueCallback());
            Action undoCallback = () => assignValueCallback(getOldValueCallback());
            RevertableChangesHandler.Do(new ProcessCommand(doCallback, undoCallback));
        }
    }
}
