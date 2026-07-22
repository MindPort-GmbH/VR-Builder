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
        private bool lockOnProcessStart;

        [SerializeField]
        private bool lockOnProcessFinished;

        public bool LockOnProcessStart
        {
            get => lockOnProcessStart;
            set => lockOnProcessStart = value;
        }

        public bool LockOnProcessFinished
        {
            get => lockOnProcessFinished;
            set => lockOnProcessFinished = value;
        }
    }
}