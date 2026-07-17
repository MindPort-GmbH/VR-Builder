// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace VRBuilder.Unity.ProcessRunning
{
    /// <summary>
    /// Initializes the <see cref="ProcessRunner"/> with the current selected process on scene start.
    /// </summary>
    [RequireComponent(typeof(DefaultProcessRunner))]
    public class InitProcessOnEnable : DefaultProcessLoader
    {
        protected void OnEnable()
        {
            StartCoroutine(StartProcess());
        }

        protected override void Start()
        {
            //empty because we want to start the process on enable
        }
    }
}