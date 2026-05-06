// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using VRBuilder.Core.Editor.UI.Drawers;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Unwraps an <see cref="IDataOwner"/> and delegates to the drawer of its <c>Data</c> member.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(IDataOwner))]
    internal class DataOwnerElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            if (value == null)
            {
                throw new NullReferenceException("Attempting to draw null IDataOwner.");
            }

            IData data = ((IDataOwner)value).Data;

            MemberInfo dataMember = MemberAccessCache.GetDataMember(value);
            if (dataMember == null)
            {
                throw new MissingFieldException($"No drawable Data member found on {value.GetType().FullName}.");
            }

            IElementDrawer dataDrawer = ElementDrawerLocator.GetDrawerForMember(dataMember, value);
            if (dataDrawer == null)
            {
                return new Label($"(no drawer for {value.GetType().Name}.Data)");
            }

            return dataDrawer.CreateElement(data, _ => changeCallback(value), label);
        }

        public override GUIContent GetLabel(object value, Type declaredType)
        {
            if (value == null)
            {
                return new GUIContent(string.Empty);
            }

            IData data = ((IDataOwner)value).Data;
            MemberInfo dataMember = MemberAccessCache.GetDataMember(value);
            if (dataMember == null)
            {
                return base.GetLabel(value, declaredType);
            }

            IElementDrawer dataDrawer = ElementDrawerLocator.GetDrawerForMember(dataMember, value);
            return dataDrawer != null ? dataDrawer.GetLabel(data, declaredType) : base.GetLabel(value, declaredType);
        }
    }
}
