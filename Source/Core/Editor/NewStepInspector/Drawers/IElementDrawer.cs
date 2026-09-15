// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// UIToolkit equivalent of <see cref="VRBuilder.Core.Editor.UI.Drawers.IProcessDrawer"/>.
    /// Instead of returning a Rect (IMGUI immediate mode), each drawer returns a
    /// VisualElement that is created once and data-bound (retained mode).
    /// </summary>
    public interface IElementDrawer
    {
        /// <summary>
        /// Creates a UIToolkit VisualElement that represents and edits the given value.
        /// </summary>
        /// <param name="currentValue">The current value to display.</param>
        /// <param name="changeValueCallback">Callback invoked when the value changes.</param>
        /// <param name="label">Display label for the element.</param>
        /// <returns>A VisualElement representing the value.</returns>
        VisualElement CreateElement(object currentValue, Action<object> changeValueCallback, string label);

        /// <summary>
        /// Returns a display label for a member, reading DisplayName and DisplayTooltip attributes.
        /// </summary>
        string GetLabel(MemberInfo memberInfo, object memberOwner);

        /// <summary>
        /// Returns a display label for a value given its declared type.
        /// </summary>
        string GetLabel(object value, Type declaredType);
    }
}
