// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using VRBuilder.Core;
using VRBuilder.Core.Conditions;
using System.Runtime.Serialization;

namespace VRBuilder.Core.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper condition for testing that allows explicitly marking a condition as completed
    /// </summary>
    [DataContract(IsReference = true)]
    public class EndlessConditionMock : Condition<EndlessConditionMock.EntityData>
    {
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            [DataMember]
            public bool IsCompleted { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public Metadata Metadata { get; set; }
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
