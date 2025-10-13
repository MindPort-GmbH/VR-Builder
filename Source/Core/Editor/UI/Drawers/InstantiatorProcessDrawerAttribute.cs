// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class InstantiatorProcessDrawerAttribute : Attribute
    {
        public Type Type { get; private set; }

        public InstantiatorProcessDrawerAttribute(Type type)
        {
            Type = type;
        }
    }
}
