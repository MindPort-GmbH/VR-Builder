// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils.Mocks;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class BackgroundBehaviorTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator NonBlockingBehaviorActivating()
        {
            // Given a non-blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(false);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it,
            behaviorMock.LifeCycle.Activate();

            // Then behavior starts its activation.
            Assert.AreEqual(Stage.Activating, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator NonBlockingBehaviorActivated()
        {
            // Given a non-blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(false);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate and finish activation,
            behaviorMock.LifeCycle.Activate();

            behaviorMock.LifeCycle.MarkToFastForward();

            yield return null;
            behaviorMock.Update();

            yield return null;
            behaviorMock.Update();

            // Then it is activated.
            Assert.AreEqual(Stage.Active, behaviorMock.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator BlockingBehaviorActivating()
        {
            // Given a blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(true);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it,
            behaviorMock.LifeCycle.Activate();

            // Then it is immediately activating.
            Assert.AreEqual(Stage.Activating, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveNonBlockingBehavior()
        {
            // Given a non-blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(false);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we mark it to fast-forward,
            behaviorMock.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it hasn't been activated yet.
            Assert.AreEqual(Stage.Inactive, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveNonBlockingBehaviorAndActivateIt()
        {
            // Given a non-blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(false);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we mark it to fast-forward and activate it,
            behaviorMock.LifeCycle.MarkToFastForward();
            behaviorMock.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBlockingBehaviorAndActivateIt()
        {
            // Given a blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(true);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we mark it to fast-forward and activate it,
            behaviorMock.LifeCycle.MarkToFastForward();
            behaviorMock.LifeCycle.Activate();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingNonBlockingBehavior()
        {
            // Given an activating non-blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(false);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            behaviorMock.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behaviorMock.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBlockingBehavior()
        {
            // Given an activating blocking behavior,
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock(true);
            behaviorMock.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            behaviorMock.LifeCycle.Activate();

            // When we mark it to fast-forward,
            behaviorMock.LifeCycle.MarkToFastForward();

            // Then the behavior should be activated immediately.
            Assert.AreEqual(Stage.Active, behaviorMock.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator NonBlockingBehaviorDoesNotBlock()
        {
            // Given a chapter with a step with no conditions but a transition to the end, and a non-blocking endless behavior,
            EndlessBehaviorMock nonBlockingBehaviorMock = new EndlessBehaviorMock(false);
            ITransition transition = new Transition();
            transition.Data.TargetStep = null;
            IStep step = new Step("NonBlockingStep");
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Data.Behaviors.Data.Behaviors.Add(nonBlockingBehaviorMock);
            IChapter chapter = new Chapter("NonBlockingChapter", step);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            // Then it will finish activation immediately after a few update cycles.
            while (chapter.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator BlockingBehaviorDoesBlock()
        {
            // Given a chapter with a step with no conditions but a transition to the end, and a blocking endless behavior,
            EndlessBehaviorMock blockingBehaviorMock = new EndlessBehaviorMock(true);
            ITransition transition = new Transition();
            transition.Data.TargetStep = null;
            IStep step = new Step("BlockingStep");
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Data.Behaviors.Data.Behaviors.Add(blockingBehaviorMock);
            IChapter chapter = new Chapter("BlockingChapter", step);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            while (blockingBehaviorMock.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                chapter.Update();
            }

            // When endless behavior stays activating even after a few frames,
            int waitingFrames = 10;
            while (waitingFrames > 0)
            {
                yield return null;
                chapter.Update();
                Assert.AreEqual(Stage.Activating, blockingBehaviorMock.LifeCycle.Stage);
                Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                waitingFrames--;
            }

            // Then the chapter will not be activated until the behavior finishes.
            blockingBehaviorMock.LifeCycle.MarkToFastForward();

            Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);

            while (chapter.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator NonBlockingBehaviorLoop()
        {
            // Given a chapter with a step with a loop transition, and a non-blocking endless behavior,
            EndlessBehaviorMock nonBlockingBehaviorMock = new EndlessBehaviorMock(false);
            ITransition transition = new Transition();
            IStep step = new Step("NonBlockingStep");
            transition.Data.TargetStep = step;
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Data.Behaviors.Data.Behaviors.Add(nonBlockingBehaviorMock);
            IChapter chapter = new Chapter("NonBlockingChapter", step);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter,
            chapter.LifeCycle.Activate();

            // Then it will loop without any problems.
            int loops = 3;
            while (loops > 0)
            {
                while (step.LifeCycle.Stage != Stage.Activating)
                {
                    yield return null;
                    chapter.Update();
                }

                while (step.LifeCycle.Stage != Stage.Active)
                {
                    yield return null;
                    chapter.Update();
                }

                while (step.LifeCycle.Stage != Stage.Deactivating)
                {
                    yield return null;
                    chapter.Update();
                }

                while (step.LifeCycle.Stage != Stage.Inactive)
                {
                    yield return null;
                    chapter.Update();
                }

                Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                loops--;
            }

            Assert.AreEqual(Stage.Inactive, step.LifeCycle.Stage);
        }
    }
}
