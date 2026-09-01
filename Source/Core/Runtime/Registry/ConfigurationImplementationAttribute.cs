// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace VRBuilder.Core.Runtime.Registry
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigurationImplementationAttribute : PropertyAttribute
    {
        public Type ConfigurationType { get; }

        public ConfigurationImplementationAttribute(Type configurationType)
        {
            ConfigurationType = configurationType;
        }
    }
}
