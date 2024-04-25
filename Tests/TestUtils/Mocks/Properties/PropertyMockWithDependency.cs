// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core.Properties;
using UnityEngine;

namespace VRBuilder.Core.Tests.Utils.Mocks
{
    /// <summary>
    /// Property requiring a <see cref="PropertyMock"/>.
    /// </summary>
    [RequireComponent(typeof(PropertyMock))]
    public class PropertyMockWithDependency : ProcessSceneObjectProperty
    {

    }
}
