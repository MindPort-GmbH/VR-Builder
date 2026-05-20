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
