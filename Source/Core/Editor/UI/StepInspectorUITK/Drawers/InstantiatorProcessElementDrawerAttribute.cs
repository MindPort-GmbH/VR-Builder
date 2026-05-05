// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Marks an <see cref="IElementDrawer"/> as a factory drawer for new instances of <see cref="Type"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class InstantiatorProcessElementDrawerAttribute : Attribute
    {
        public Type Type { get; }

        public InstantiatorProcessElementDrawerAttribute(Type type)
        {
            Type = type;
        }
    }
}
