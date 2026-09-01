// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.Utils;

namespace VRBuilder.Editor.Registry
{
    [CustomPropertyDrawer(typeof(ServiceImplementationAttribute))]
    public class ServiceImplementationDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, Type[]> typeCache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. Resolve the interface type from the attribute
            var interfaceType = ((ServiceImplementationAttribute)attribute).InterfaceType;

            // 2. find all concrete implementations and cache them
            if (!typeCache.TryGetValue(interfaceType, out var implementations))
            {
                implementations = ReflectionUtils
                    .GetConcreteImplementationsOf(interfaceType)
                    .OrderBy(t => t.Name) // alphabetical
                    .ToArray();
                typeCache[interfaceType] = implementations;
            }

            // 3. Build display names (short) and stored values (FullName)
            var displayNames = implementations.Select(t => t.Name).ToArray();
            var fullNames = implementations.Select(t => t.AssemblyQualifiedName).ToArray();

            // 4. Find index of currently stored value
            var selectedIndex = string.IsNullOrEmpty(property.stringValue)
                ? -1
                : Array.IndexOf(fullNames, property.stringValue);

            // auto fix if current value is missing
            if (selectedIndex < 0 && fullNames.Length > 0)
            {
                selectedIndex = 0;
                property.stringValue = fullNames[0];
            }

            // 5. Draw label + popup
            EditorGUI.BeginProperty(position, label, property);
            var popupRect = EditorGUI.PrefixLabel(position, label);
            var newIndex = EditorGUI.Popup(popupRect, selectedIndex, displayNames);

            // 6. write back the change
            if (newIndex != selectedIndex && newIndex >= 0)
            {
                property.stringValue = fullNames[newIndex];
            }

            EditorGUI.EndProperty();
        }
    }
}
