// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Properties;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper condition for testing that allows explicitly marking a condition as completed.
    /// It can reference a <see cref="PropertyMock"/>.
    /// </summary>
    public class ReferencingConditionMock : Condition<ReferencingConditionMock.EntityData>
    {
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            [DataMember]
            public bool IsCompleted { get; set; }

            [DataMember]
            public ScenePropertyReference<PropertyMock> PropertyMock { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public Metadata Metadata { get; set; }
            public IData ParentData { get; set; }
        }

        private class ActiveProcess : InstantProcess<EntityData>
        {
            public override void Start()
            {
                Data.IsCompleted = false;
            }

            public ActiveProcess(EntityData data) : base(data)
            {
            }
        }

        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }
    }
}
