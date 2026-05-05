// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

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
