// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils.Builders;
using VRBuilder.Core.Tests.Utils.Mocks;

namespace VRBuilder.Core.Tests.Processes
{
    public class LifeCycleTests : RuntimeTests
    {
        private class TestEntity : Entity<TestEntity.EntityData>
        {
            public class EntityData : IData, IModeData
            {
                public Metadata Metadata { get; set; }

                public bool IsUpdateFinished { get; set; }
                public bool IsFastForwarded { get; set; }
                public bool IsEndCalled { get; set; }
                public IMode Mode { get; set; }
            }

            private class ActiveProcess : StageProcess<EntityData>
            {
                public ActiveProcess(EntityData data) : base(data)
                {
                }

                public override void Start()
                {
                    Data.IsUpdateFinished = false;
                    Data.IsFastForwarded = false;
                }

                public override IEnumerator Update()
                {
                    Data.IsUpdateFinished = true;
                    yield break;
                }

                public override void End()
                {
                    Data.IsEndCalled = true;
                }

                public override void FastForward()
                {
                    Data.IsFastForwarded = true;
                }
            }

            public override IStageProcess GetActiveProcess()
            {
                return new ActiveProcess(Data);
            }
        }

        [UnityTest]
        public IEnumerator EntityIsAborted()
        {
            // Given a running entity,
            IEntity entity = new EndlessBehaviorMock();

            entity.LifeCycle.Activate();

            Assert.AreEqual(Stage.Activating, entity.LifeCycle.Stage);

            // When it is aborted,
            entity.LifeCycle.Abort();

            // Then it goes back to inactive.
            Assert.AreEqual(Stage.Aborting, entity.LifeCycle.Stage);

            entity.LifeCycle.Update();
            yield return null;

            Assert.AreEqual(Stage.Inactive, entity.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator FastForwardNextStage()
        {
            // Given an entity,
            IEntity entity = new EndlessBehaviorMock();

            entity.LifeCycle.Activate();

            Assert.AreEqual(Stage.Activating, entity.LifeCycle.Stage);

            // When you fast-forward its next stage,
            entity.LifeCycle.MarkToFastForwardStage(Stage.Active);

            // Then the current stage is not fast-forwarded.
            Assert.AreEqual(Stage.Activating, entity.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardCompletedProcess()
        {
            // Given an entity in an active stage with exhausted process,
            TestEntity entity = new TestEntity();
            entity.LifeCycle.Activate();
            entity.Update();
            Assert.AreEqual(Stage.Active, entity.LifeCycle.Stage);

            int endlessLoopProtection = 0;

            while (entity.Data.IsUpdateFinished == false && endlessLoopProtection < 30)
            {
                entity.LifeCycle.Update();
            }

            Assert.IsTrue(endlessLoopProtection < 30);
            Assert.IsTrue(entity.Data.IsEndCalled);
            Assert.IsFalse(entity.Data.IsFastForwarded);
            Assert.AreEqual(Stage.Active, entity.LifeCycle.Stage);

            // When we fast-forward it,
            entity.LifeCycle.MarkToFastForwardStage(Stage.Active);

            // Nothing happens.
            Assert.IsFalse(entity.Data.IsFastForwarded);

            yield break;
        }

        [UnityTest]
        public IEnumerator StepContinuesIfChildAborted()
        {
            // Given a step with a behavior,
            IBehavior behavior = new EndlessBehaviorMock();
            BasicStepBuilder stepBuilder = new BasicStepBuilder("TestStep")
                .AddBehavior(behavior);

            Step step = stepBuilder.Build();
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            step.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                step.Update();
            }

            // If the behavior is aborted,
            behavior.LifeCycle.Abort();

            while (behavior.LifeCycle.Stage == Stage.Aborting)
            {
                yield return null;
                step.Update();
            }

            // Then the step continues execution.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, step.LifeCycle.Stage);

            yield return null;
            step.Update();
            yield return null;
            step.Update();
            yield return null;
            step.Update();
            yield return null;
            step.Update();
            yield return null;
            step.Update();

            Assert.AreEqual(Stage.Active, step.LifeCycle.Stage);

        }
    }
}
