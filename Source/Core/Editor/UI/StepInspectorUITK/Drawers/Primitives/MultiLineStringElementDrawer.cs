using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers.Primitives
{
    /// <summary>
    /// Multi-line variant of <see cref="StringElementDrawer"/>. Picked up via
    /// <c>[UsesSpecificProcessDrawer("MultiLineStringDrawer")]</c> on a member
    /// (the locator does a partial-name match on the drawer type).
    /// </summary>
    [DefaultProcessElementDrawer(typeof(MultiLineString))]
    internal class MultiLineStringDrawer : ElementDrawer
    {
        public override VisualElement CreateElement(object value, Action<object> changeCallback, GUIContent label)
        {
            string oldValue = value as string ?? string.Empty;

            TextField field = new TextField(label?.text)
            {
                value = oldValue,
                tooltip = label?.tooltip,
                isDelayed = true,
                multiline = true
            };
            field.AddToClassList("vrb-field");
            field.AddToClassList("vrb-field--string");
            field.AddToClassList("vrb-field--multiline");
            field.style.whiteSpace = WhiteSpace.Normal;

            field.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                string committedNew = evt.newValue ?? string.Empty;
                string committedOld = oldValue;
                if (committedNew == committedOld)
                {
                    return;
                }

                ChangeValue(
                    getNewValueCallback: () => committedNew,
                    getOldValueCallback: () => committedOld,
                    assignValueCallback: changeCallback);

                oldValue = committedNew;
            });

            return field;
        }
    }

    // Marker type — never instantiated. Exists so the locator's name-based
    // resolution of [UsesSpecificProcessDrawer("MultiLineStringDrawer")] finds it.
    internal sealed class MultiLineString { private MultiLineString() { } }
}
