// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.Settings;
using VRBuilder.Core.StepLocking;

namespace VRBuilder.Core.RestrictiveEnvironment
{
    [CreateAssetMenu(fileName = "StepLockSettings", menuName = "VR Builder/Step Lock Settings", order = 2)]
    public class StepLockSettings : SettingsObject<StepLockSettings>, IStepLockConfiguration
    {
        [SerializeField]
        private string serviceTypeName = typeof(DefaultStepLockHandling).FullName;

        public string ServiceTypeName => serviceTypeName;

        public bool LockOnProcessStart { get; set; }
        public bool LockOnProcessFinished { get; set; }
    }
}