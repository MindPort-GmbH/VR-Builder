// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Editor.UI.Drawers;
using VRBuilder.Core.Editor.UndoRedo;
using VRBuilder.Core.Utils;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// Abstract base class for UIToolkit element drawers.
    /// Equivalent of <see cref="VRBuilder.Core.Editor.UI.Drawers.AbstractDrawer"/>.
    /// Provides undo/redo support via ProcessCommand and label generation from attributes.
    /// </summary>
    public abstract class ElementDrawer : IElementDrawer
    {
        /// <inheritdoc />
        public abstract VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label);

        /// <inheritdoc />
        public virtual string GetLabel(MemberInfo memberInfo, object memberOwner)
        {
            Type memberType = MemberAccessCache.GetDeclaredType(memberInfo);
            object value = MemberAccessCache.GetValue(memberOwner, memberInfo);

            if (value != null)
            {
                memberType = value.GetType();
            }

            string result = GetLabel(value, memberType);

            DisplayNameAttribute displayNameAttribute = memberInfo.GetAttributes<DisplayNameAttribute>(true).FirstOrDefault();
            DisplayTooltipAttribute displayTooltipAttribute = memberInfo.GetAttributes<DisplayTooltipAttribute>(true).FirstOrDefault();

            if (displayNameAttribute != null && displayNameAttribute.Name != null)
            {
                result = displayNameAttribute.Name;
            }

            if (string.IsNullOrEmpty(result))
            {
                result = memberInfo.Name;
            }

            return result;
        }

        /// <inheritdoc />
        public virtual string GetLabel(object value, Type declaredType)
        {
            INamedData nameable = value as INamedData;

            if (nameable != null && !string.IsNullOrEmpty(nameable.Name))
            {
                return nameable.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// Wraps a value change in a ProcessCommand for undo/redo support.
        /// Mirrors AbstractDrawer.ChangeValue exactly.
        /// </summary>
        protected void ChangeValue(Func<object> getNewValueCallback, Func<object> getOldValueCallback, Action<object> assignValueCallback)
        {
            Action doCallback = () => assignValueCallback(getNewValueCallback());
            Action undoCallback = () => assignValueCallback(getOldValueCallback());
            RevertableChangesHandler.Do(new ProcessCommand(doCallback, undoCallback));
        }
    }
}
