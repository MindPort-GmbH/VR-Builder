// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core.Properties;
using UnityEngine;

namespace VRBuilder.Tests.Utils.Mocks
{
    /// <summary>
    /// Property requiring a <see cref="LockablePropertyMock"/>.
    /// </summary>
    [RequireComponent(typeof(LockablePropertyMock))]
    public class LockablePropertyMockWithDependency : LockableProperty
    {
        protected override void InternalSetLocked(bool lockState)
        {
            GetComponent<LockablePropertyMock>().SetLocked(lockState);
        }
    }
}
