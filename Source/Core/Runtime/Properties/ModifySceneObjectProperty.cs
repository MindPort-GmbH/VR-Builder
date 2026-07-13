// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;

namespace VRBuilder.Core.Runtime.Source.Core.Runtime.Properties
{
    public class ModifySceneObjectProperty : ProcessSceneObjectProperty, IModifySceneObjectProperty
    {
        public void SetActive(bool setEnabled)
        {
            RuntimeConfigurator.Configuration.SceneObjectManager.SetSceneObjectActive(SceneObject, setEnabled);
        }
    }
}