// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections.Generic;
using System.Runtime.Serialization;
using VRBuilder.Core;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper condition for testing that allows explicitly marking a condition as completed.
    /// It can reference a <see cref="LockablePropertyMock"/>.
    /// </summary>
    public class LockableReferencingConditionMock : Condition<LockableReferencingConditionMock.EntityData>
    {
        public IEnumerable<LockablePropertyData> LockableProperties = null;

        [DataContract(IsReference = true)]
        public class EntityData : IConditionData
        {
            [DataMember]
            public bool IsCompleted { get; set; }

            [DataMember]
            public ScenePropertyReference<ILockablePropertyMock> LockablePropertyMock { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
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

        public override IEnumerable<LockablePropertyData> GetLockableProperties()
        {
            if (LockableProperties == null)
            {
                return base.GetLockableProperties();
            }

            return LockableProperties;
        }

        public override IStageProcess GetActiveProcess()
        {
            return new ActiveProcess(Data);
        }
    }
}
