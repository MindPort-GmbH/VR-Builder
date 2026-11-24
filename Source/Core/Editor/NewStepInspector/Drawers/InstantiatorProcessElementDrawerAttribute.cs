// Copyright (c) 2021-2025 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// Marks a class as a UIToolkit instantiator drawer for a specific type.
    /// Mirrors <see cref="VRBuilder.Core.Editor.UI.Drawers.InstantiatorProcessDrawerAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InstantiatorProcessElementDrawerAttribute : Attribute
    {
        /// <summary>
        /// The type this instantiator creates.
        /// </summary>
        public Type Type { get; private set; }

        public InstantiatorProcessElementDrawerAttribute(Type type)
        {
            Type = type;
        }
    }
}
