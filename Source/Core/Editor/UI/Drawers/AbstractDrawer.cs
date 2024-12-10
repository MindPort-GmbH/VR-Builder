// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Linq;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Editor.UndoRedo;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Simple base drawer class.
    /// </summary>
    public abstract class AbstractDrawer : IProcessDrawer
    {
        /// <inheritdoc />
        public Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, string label)
        {
            return Draw(rect, currentValue, changeValueCallback, new GUIContent(label));
        }

        /// <inheritdoc />
        public abstract Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label);

        public virtual GUIContent GetLabel(MemberInfo memberInfo, object memberOwner)
        {
            Type memberType = ReflectionUtils.GetDeclaredTypeOfPropertyOrField(memberInfo);
            object value = ReflectionUtils.GetValueFromPropertyOrField(memberOwner, memberInfo);

            if (value != null)
            {
                memberType = value.GetType();
            }

            GUIContent valueLabel = GetLabel(value, memberType);

            DisplayNameAttribute displayNameAttribute = memberInfo.GetAttributes<DisplayNameAttribute>(true).FirstOrDefault();
            DisplayTooltipAttribute displayTooltipAttribute = memberInfo.GetAttributes<DisplayTooltipAttribute>(true).FirstOrDefault();

            if (displayNameAttribute != null && displayNameAttribute.Name != null)
            {
                valueLabel.text = displayNameAttribute.Name;
            }

            if(displayTooltipAttribute != null && displayTooltipAttribute.Tooltip != null)
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

        /// <inheritdoc />
        public void ChangeValue(Func<object> getNewValueCallback, Func<object> getOldValueCallback, Action<object> assignValueCallback)
        {
            // ReSharper disable once ImplicitlyCapturedClosure
            Action doCallback = () => assignValueCallback(getNewValueCallback());
            // ReSharper disable once ImplicitlyCapturedClosure
            Action undoCallback = () => assignValueCallback(getOldValueCallback());
            RevertableChangesHandler.Do(new ProcessCommand(doCallback, undoCallback));
        }
    }
}
