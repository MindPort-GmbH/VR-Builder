// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Reflection;
using VRBuilder.Core;
using UnityEngine;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    [DefaultProcessDrawer(typeof(IDataOwner))]
    internal class DataOwnerDrawer : AbstractDrawer
    {
        public override Rect Draw(Rect rect, object currentValue, Action<object> changeValueCallback, GUIContent label)
        {
            if (currentValue == null)
            {
                throw new NullReferenceException("Attempting to draw null object.");
            }

            IData data = ((IDataOwner)currentValue).Data;

            MemberInfo dataMember = MemberAccessCache.GetDataMember(currentValue);
            if (dataMember == null)
            {
                throw new MissingFieldException($"No drawable Data member found on {currentValue.GetType().FullName}.");
            }

            IProcessDrawer dataDrawer = DrawerLocator.GetDrawerForMember(dataMember, currentValue);

            return dataDrawer.Draw(rect, data, (value) => changeValueCallback(currentValue), label);
        }

        public override GUIContent GetLabel(object value, Type declaredType)
        {
            IData data = ((IDataOwner)value).Data;

            MemberInfo dataMember = MemberAccessCache.GetDataMember(value);
            if (dataMember == null)
            {
                throw new MissingFieldException($"No drawable Data member found on {value.GetType().FullName}.");
            }

            IProcessDrawer dataDrawer = DrawerLocator.GetDrawerForMember(dataMember, value);
            return dataDrawer.GetLabel(data, declaredType);
        }
    }
}
