// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.ProcessRunning;

namespace VRBuilder.Unity.ProcessRunning
{
    /// <summary>
    /// Initializes the <see cref="ProcessRunner"/> with the current selected process on scene start.
    /// </summary>
    [RequireComponent(typeof(DefaultProcessRunner))]
    public class InitProcessOnEnable : DefaultProcessHandler
    {
        protected void OnEnable()
        {
            LoadAndStartProcess();
        }
    }
}
