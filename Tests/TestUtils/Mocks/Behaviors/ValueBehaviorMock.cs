// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core;
using VRBuilder.Core.Behaviors;

namespace VRBuilder.Core.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper behavior for testing which just has a float value.
    /// </summary>
    public class ValueBehaviorMock : Behavior<ValueBehaviorMock.EntityData>
    {
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Value which can be set.
            /// </summary>
            public float Value { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        public ValueBehaviorMock(float value)
        {
            Data.Value = value;
        }
    }
}
