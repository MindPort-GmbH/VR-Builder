// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Runtime.Properties
{
    public class ModifySceneComponentProperty : ProcessSceneObjectProperty, IModifySceneComponentProperty
    {
        public void SetComponentActive(string componentTypeName, bool setEnabled)
        {
            IEnumerable<Component> components = SceneObject.GameObject().GetComponents<Component>().Where(c => c.GetType().Name == componentTypeName);

            foreach (Component component in components)
            {
                Type componentType = component.GetType();

                if (componentType.GetProperty("enabled") != null)
                {
                    componentType.GetProperty("enabled").SetValue(component, setEnabled, null);
                }
            }
        }
    }
}