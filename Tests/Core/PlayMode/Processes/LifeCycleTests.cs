// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Exceptions;
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

        [UnityTest]
        public IEnumerator NoChangeInChapterIfCurrentStepIsAborted()
        {
            // Given a chapter,            
            LinearChapterBuilder chapterBuilder = new LinearChapterBuilder("TestChapter")
                .AddStep(new BasicStepBuilder("1")
                    .AddBehavior(new EndlessBehaviorMock()))
                .AddStep(new BasicStepBuilder("2")
                    .AddBehavior(new EndlessBehaviorMock()));

            IChapter chapter = chapterBuilder.Build();
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            chapter.LifeCycle.Activate();

            yield return null;
            chapter.Update();

            Assert.AreEqual("1", chapter.Data.Current.Data.Name);
            Assert.AreEqual(Stage.Activating, chapter.Data.Current.LifeCycle.Stage);

            // If the current step is aborted,
            chapter.Data.Current.LifeCycle.Abort();

            while (chapter.Data.Current.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                chapter.Update();
            }

            // Then nothing changes in the state of the chapter.
            Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
            Assert.AreEqual("1", chapter.Data.Current.Data.Name);
        }

        [UnityTest]
        public IEnumerator ProcessMovesToNextChapterIfChapterIsAborted()
        {
            // Given a process,
            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("C1")
                    .AddStep(new BasicStepBuilder("1").AddBehavior(new EndlessBehaviorMock())))
                .AddChapter(new LinearChapterBuilder("C2")
                    .AddStep(new BasicStepBuilder("2").AddBehavior(new EndlessBehaviorMock())))
                .Build();
            process.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            process.LifeCycle.Activate();

            yield return null;
            process.Update();

            Assert.AreEqual(Stage.Activating, process.Data.Current.LifeCycle.Stage);

            // If a chapter is aborted,
            process.Data.Current.LifeCycle.Abort();

            while (process.Data.Current.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                process.Update();
            }

            yield return null;
            process.Update();

            // Then the process moves to the next chapter.
            Assert.AreEqual("C2", process.Data.Current.Data.Name);
            Assert.AreEqual(Stage.Activating, process.Data.Current.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator ExceptionThrownWhenAbortingInactiveEntity()
        {
            // Given an inactive entity,
            IBehavior behavior = new EndlessBehaviorMock();

            // When it is aborted,
            TestDelegate abort = new TestDelegate(() => behavior.LifeCycle.Abort());

            // An exception is thrown.
            Assert.Throws<InvalidStateException>(abort);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ExceptionThrownWhenAbortingAbortingEntity()
        {
            // Given an aborting entity,
            IBehavior behavior = new EndlessBehaviorMock();
            behavior.LifeCycle.Activate();

            yield return null;
            behavior.LifeCycle.Update();

            Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);

            behavior.LifeCycle.Abort();

            // When it is aborted,
            TestDelegate abort = new TestDelegate(() => behavior.LifeCycle.Abort());

            // An exception is thrown.
            Assert.Throws<InvalidStateException>(abort);
        }
    }
}
