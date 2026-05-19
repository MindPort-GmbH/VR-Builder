// Copyright (c) 2021-2025 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// Marks a class as the default UIToolkit element drawer for a specific type.
    /// Mirrors <see cref="VRBuilder.Core.Editor.UI.Drawers.DefaultProcessDrawerAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultProcessElementDrawerAttribute : Attribute
    {
        /// <summary>
        /// The type this drawer can draw.
        /// </summary>
        public Type DrawableType { get; private set; }

        public DefaultProcessElementDrawerAttribute(Type type)
        {
            DrawableType = type;
        }
    }
}
