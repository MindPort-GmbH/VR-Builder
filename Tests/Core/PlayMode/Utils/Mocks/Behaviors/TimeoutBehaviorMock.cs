// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.Collections;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using UnityEngine;

namespace VRBuilder.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper Behavior for testing that provides a behavior with fixed durations for activate and deactivate
    /// </summary>
    public class TimeoutBehaviorMock : Behavior<TimeoutBehaviorMock.EntityData>
    {
        public class EntityData : IBehaviorData
        {
            public float ActivatingTime { get; set; }
            public float DeactivatingTime { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        public TimeoutBehaviorMock(float activatingTime, float deactivatingTime)
        {
            Data.ActivatingTime = activatingTime;
            Data.DeactivatingTime = deactivatingTime;
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            public override void Start()
            {
            }

            public override IEnumerator Update()
            {
                float startedAt = Time.time;

                while (Time.time - startedAt < Data.ActivatingTime)
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

        private class DeactivatingProcess : StageProcess<EntityData>
        {
            public DeactivatingProcess(EntityData data) : base(data)
            {
            }

            public override void Start()
            {
            }

            public override IEnumerator Update()
            {
                float startedAt = Time.time;

                while (Time.time - startedAt < Data.DeactivatingTime)
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

        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }

        public override IStageProcess GetDeactivatingProcess()
        {
            return new DeactivatingProcess(Data);
        }
    }
}
