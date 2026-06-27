using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Interface of a drawer that builds the editor UI for a process member as a
    /// <see cref="VisualElement"/> tree.
    /// </summary>
    /// <remarks>
    /// Implementations must be stateless and reentrant: a single instance is resolved once per
    /// type and shared across every element it draws. Per-element state belongs in
    /// <see cref="CreateElement"/>'s locals/closures, never in instance fields — a mutable field
    /// would bleed between unrelated elements.
    /// </remarks>
    public interface IElementDrawer
    {
        /// <summary>
        /// Build the editor UI for a member and return its root element.
        /// </summary>
        /// <param name="value">Current value of the member.</param>
        /// <param name="changeCallback">
        /// Delegate for a method that changes the value of the member. Done that way to allow non-instantaneous assignments (for example, from generic menus).
        /// Invoke only when the value (or values of child members) has actually changed.
        /// </param>
        /// <param name="label">Label content to display.</param>
        /// <returns>The root <see cref="VisualElement"/> of the drawn member.</returns>
        VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label);

        /// <summary>
        /// Return a label for a property/field <paramref name="memberInfo"/> of an object <paramref name="memberOwner"/>.
        /// </summary>
        GUIContent GetLabel(MemberInfo memberInfo, object memberOwner);

        /// <summary>
        /// Return a label for a <paramref name="value"/> of <paramref name="declaredType"/>.
        /// </summary>
        GUIContent GetLabel(object value, Type declaredType);
    }
}
