// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace VRBuilder.Core.Runtime.Registry
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ServiceImplementationAttribute : PropertyAttribute
    {
        public Type InterfaceType { get; }

        public ServiceImplementationAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }
    }
}