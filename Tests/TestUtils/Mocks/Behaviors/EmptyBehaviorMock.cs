// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core.Behaviors;

namespace VRBuilder.Core.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper behavior for testing that does nothing
    /// </summary>
    public class EmptyBehaviorMock : Behavior<EmptyBehaviorMock.EntityData>
    {
        public class EntityData : IBehaviorData
        {
            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }
    }
}
