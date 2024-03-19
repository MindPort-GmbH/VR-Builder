// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.Collections;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration.Modes;
using UnityEngine;

namespace VRBuilder.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper behavior for testing which has an activation state.
    /// </summary>
    public class ActivationStageBehaviorMock : Behavior<ActivationStageBehaviorMock.EntityData>, IOptional
    {
        public class EntityData : IBehaviorData
        {
            public BehaviorExecutionStages ExecutionStages { get; set; }

            public AudioSource AudioPlayer { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        private class HasExecutionStageProcess : StageProcess<EntityData>
        {
            private BehaviorExecutionStages executionStages;

            public HasExecutionStageProcess(BehaviorExecutionStages executionStages, EntityData data) : base(data)
            {
                this.executionStages = executionStages;
            }

            public override void Start()
            {
            }

            public override IEnumerator Update()
            {
                if ((Data.ExecutionStages & executionStages) > 0)
                {
                    yield return null;
                }
            }

            public override void End()
            {
            }

            public override void FastForward()
            {
            }
        }

        public ActivationStageBehaviorMock(BehaviorExecutionStages executionStages, string name = "Activation Stage Mock")
        {
            Data.ExecutionStages = executionStages;
            Data.Name = name;
        }

        public override IStageProcess GetActivatingProcess()
        {
            return new HasExecutionStageProcess(BehaviorExecutionStages.Activation, Data);
        }

        public override IStageProcess GetDeactivatingProcess()
        {
            return new HasExecutionStageProcess(BehaviorExecutionStages.Deactivation, Data);
        }
    }
}
