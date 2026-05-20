using System;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Marks an <see cref="IElementDrawer"/> as the default drawer for the given type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class DefaultProcessElementDrawerAttribute : Attribute
    {
        public Type DrawableType { get; }

        public DefaultProcessElementDrawerAttribute(Type type)
        {
            DrawableType = type;
        }
    }
}
