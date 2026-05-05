// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// UIToolkit counterpart of <see cref="VRBuilder.Core.Editor.UI.Drawers.IProcessDrawer"/>.
    /// </summary>
    public interface IElementDrawer
    {
        VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label);

        GUIContent GetLabel(MemberInfo memberInfo, object memberOwner);

        GUIContent GetLabel(object value, Type declaredType);
    }
}
