// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core.Configuration.Modes;

namespace VRBuilder.Core.Tests.Utils.Mocks
{
    public class OptionalEndlessBehaviorMock : EndlessBehaviorMock, IOptional
    {
        public OptionalEndlessBehaviorMock(bool isBlocking = true) : base(isBlocking)
        {
        }
    }
}
