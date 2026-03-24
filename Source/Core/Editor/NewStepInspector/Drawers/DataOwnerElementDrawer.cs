// Copyright (c) 2021-2025 MindPort GmbH

using System;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit port of <see cref="VRBuilder.Core.Editor.UI.Drawers.DataOwnerDrawer"/>.
    /// Unwraps IDataOwner to its Data member and delegates to the appropriate drawer.
    /// </summary>
    [DefaultProcessElementDrawer(typeof(IDataOwner))]
    public class DataOwnerElementDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label)
        {
            IDataOwner dataOwner = (IDataOwner)currentValue;
            IData data = dataOwner.Data;

            IElementDrawer dataDrawer = ElementDrawerLocator.GetDrawerForValue(data, data.GetType());
            return dataDrawer.CreateElement(data, (newValue) => changeValueCallback(currentValue), label);
        }
    }
}
