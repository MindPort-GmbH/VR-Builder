// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Runtime.Serialization;
using VRBuilder.Core;
using VRBuilder.Core.Conditions;

namespace VRBuilder.Tests.Utils.Mocks
{
    [DataContract(IsReference = true)]
    public class AutoCompletedCondition : Condition<AutoCompletedCondition.EntityData>
    {
        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            public bool IsCompleted { get; set; }

            public string Name { get; set; }

            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            public IData ParentData { get; set; }
        }

        private class ActiveProcess : InstantProcess<EntityData>
        {
            public ActiveProcess(EntityData data) : base(data)
            {
            }

            public override void Start()
            {
                Data.IsCompleted = true;
            }
        }

        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }
    }
}
