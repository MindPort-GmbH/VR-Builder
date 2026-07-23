// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Properties;
using VRBuilder.Core.Runtime.Registry;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Utils
{
    /// <summary>
    /// Handles locking of all process objects in the scene and makes them non-interactable before the process is started.
    /// </summary>
    public class LockObjectsOnSceneStart : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Lock all process objects in the scene and makes them non-interactable before the process is started.")]
        private bool lockSceneObjectsOnSceneStart = true;

        // Start is called before the first frame update
        private void Start()
        {
            foreach(ILockableProperty lockable in ServiceRegistry.Get<ISceneObjectRegistry>().GetAllProperties<ILockableProperty>())
            {
                if(lockable.InheritSceneObjectLockState && !lockable.IsAlwaysUnlocked)
                {
                    lockable.SetLocked(lockSceneObjectsOnSceneStart);
                }
            }
        }
    }
}
