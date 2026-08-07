// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Runtime.Properties
{
    public class ModifySceneComponentProperty : ProcessSceneObjectProperty, IModifySceneComponentProperty
    {
        public void SetComponentActive(string componentType, bool setEnabled)
        {
            RuntimeConfigurator.Configuration.SceneObjectManager.SetComponentActive(SceneObject, componentType, setEnabled);
        }
    }
}