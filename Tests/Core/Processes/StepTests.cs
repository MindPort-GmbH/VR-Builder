// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021 MindPort GmbH

using System.Collections;
using UnityEngine.TestTools;
using System;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using UnityEngine;
using NUnit.Framework;

namespace VRBuilder.Tests.Processes
{
    public class StepTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator StepIsSetup()
        {
            Step step = new Step("Step1");

            // Name should be added to the chapter.
            Assert.AreEqual("Step1", step.Data.Name);

            // State is correct
            Assert.AreEqual(Stage.Inactive, step.LifeCycle.Stage, "Chapter should be inactive");

            // Has transitions and behaviours
            Assert.IsNotNull(step.Data.Behaviors, "Behaviors list should be initialized");
            Assert.IsNotNull(step.Data.Transitions.Data.Transitions, "Transitions list should be initialized");

            yield return null;
        }

        [UnityTest]
        public IEnumerator DeactivateWhileNotActive()
        {
            Step step = new Step("Step1");

            bool didNotFail = false;
            bool isWrongException = false;

            // Expect to fail on calling Deactivate() before activating the Chapter.
            try
            {
                step.LifeCycle.Deactivate();
                didNotFail = true;
            }
            catch (InvalidStateException)
            {
                // This is ok
            }
            catch (Exception)
            {
                isWrongException = true;
            }

            Assert.IsFalse(didNotFail, "No Exception was raised!");
            Assert.IsFalse(isWrongException, "Wrong Exception was raised!");

            yield return null;
        }

        [UnityTest]
        public IEnumerator EmptyStepRaisesEvents()
        {
            Step step = new Step("Step1");
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            bool wasActivated = false;
            bool wasDeactivated = false;

            step.LifeCycle.StageChanged += (sender, args) =>
            {
                if (args.Stage == Stage.Active)
                {
                    wasActivated = true;
                }

                if (args.Stage == Stage.Inactive)
                {
                    wasDeactivated = true;
                }
            };

            // Activate should work on simple steps.
            step.LifeCycle.Activate();

            while (step.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            step.LifeCycle.Deactivate();

            while (step.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                step.Update();
            }

            // Step should be completed, and have become activated and deactivated.
            Assert.IsTrue(wasActivated, "Step was not activated");
            Assert.IsTrue(wasDeactivated, "Step was not deactivated");
        }

        [UnityTest]
        public IEnumerator ActivateEventEmitted()
        {
            // Setup Step with event listener for checking states.
            Step step = new Step("Step1");
            bool isActivated = false;
            bool isActivationStarted = false;

            step.LifeCycle.StageChanged += (sender, args) =>
            {
                if (args.Stage == Stage.Active)
                {
                    isActivated = true;
                }

                if (args.Stage == Stage.Activating)
                {
                    isActivationStarted = true;
                }
            };

            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // Activate should work on simple steps.
            step.LifeCycle.Activate();

            while (step.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            Assert.IsTrue(isActivationStarted, "Step was not activated");
            Assert.IsTrue(isActivated, "Step was not set to active");
        }

        [UnityTest]
        public IEnumerator StepWithCondition()
        {
            Step step = new Step("Step1");
            EndlessConditionMock conditionMock = new EndlessConditionMock();
            Transition transition = new Transition();
            transition.Data.Conditions.Add(conditionMock);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            step.LifeCycle.Activate();

            while (step.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            Stage stepInitialStage = step.LifeCycle.Stage;
            Stage conditionInitialStage = conditionMock.LifeCycle.Stage;
            bool conditionIsCompletedInitially = conditionMock.IsCompleted;

            conditionMock.Autocomplete();

            bool conditionIsCompleted = conditionMock.IsCompleted;

            step.LifeCycle.Deactivate();

            while (step.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
                step.Update();
            }

            Stage stepStageInEnd = step.LifeCycle.Stage;
            Stage conditionStageInEnd = conditionMock.LifeCycle.Stage;
            bool conditionIsCompletedInEnd = conditionMock.IsCompleted;

            // Check states were correct
            Assert.AreEqual(Stage.Active, stepInitialStage, "Step should be active initially");
            Assert.AreEqual(Stage.Active, conditionInitialStage, "Condition should be active initially");
            Assert.IsFalse(conditionIsCompletedInitially, "Condition should not completed initially");

            Assert.IsTrue(conditionIsCompleted, "Condition should be completed now");

            Assert.AreEqual(Stage.Inactive, stepStageInEnd, "Step should be inactive in the end");
            Assert.AreEqual(Stage.Inactive, conditionStageInEnd, "Condition should not be active in the end");
            Assert.IsTrue(conditionIsCompletedInEnd, "Condition should be completed in the end");

            yield return null;
        }

        [UnityTest]
        public IEnumerator StepWithInitiallyCompletedConditionResetsCondition()
        {
            // Given a step with an already completed condition,
            Step step = new Step("Step1");
            ICondition condition = new EndlessConditionMock();
            condition.Autocomplete();
            Transition transition = new Transition();
            transition.Data.Conditions.Add(condition);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When it is activated,
            step.LifeCycle.Activate();

            while (condition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            // Then the condition is reset to not completed.
            Assert.IsFalse(condition.IsCompleted);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ConditionsActivateOnlyAfterBehaviors()
        {
            Step step = new Step("Step1");
            EndlessConditionMock conditionMock = new EndlessConditionMock();
            EndlessBehaviorMock behaviorMock = new EndlessBehaviorMock();
            Transition transition = new Transition();
            transition.Data.Conditions.Add(conditionMock);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Data.Behaviors.Data.Behaviors.Add(behaviorMock);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            step.LifeCycle.Activate();

            while (behaviorMock.LifeCycle.Stage != Stage.Activating)
            {
                Assert.AreEqual(Stage.Activating, step.LifeCycle.Stage);
                Assert.AreEqual(Stage.Inactive, conditionMock.LifeCycle.Stage);
                yield return null;
                step.Update();
            }

            behaviorMock.LifeCycle.MarkToFastForwardStage(Stage.Activating);

            while (conditionMock.LifeCycle.Stage != Stage.Active)
            {
                Assert.AreEqual(Stage.Activating, step.LifeCycle.Stage);
                yield return null;
                step.Update();
            }
        }

        [UnityTest]
        public IEnumerator ActivateTest()
        {
            // Setup Step with event listener for checking states.
            Step step = new Step("Step1");
            EndlessConditionMock conditionMock = new EndlessConditionMock(); // add condition to prevent step from auto-completing on activation

            Transition transition = new Transition();
            transition.Data.Conditions.Add(conditionMock);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // Activate should work on simple steps.
            step.LifeCycle.Activate();

            while (step.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                step.Update();
            }

            // Chapter should be active now.
            Assert.AreEqual(Stage.Active, step.LifeCycle.Stage, "Step was not activated");
        }

        [UnityTest]
        public IEnumerator FastForwardInactive()
        {
            // Given a step,
            IBehavior behavior = new EndlessBehaviorMock();
            ICondition condition = new EndlessConditionMock();
            Transition transition = new Transition();
            IStep step = new Step("Step");
            transition.Data.Conditions.Add(condition);
            step.Data.Behaviors.Data.Behaviors.Add(behavior);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When you fast-forward it,
            step.LifeCycle.MarkToFastForward();

            // Then it doesn't change it's activation state, as well as its contents.
            Assert.AreEqual(Stage.Inactive, step.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, condition.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveAndActivate()
        {
            // Given a step,
            IBehavior behavior = new EndlessBehaviorMock();
            ICondition condition = new EndlessConditionMock();
            Transition transition = new Transition();
            IStep step = new Step("Step");
            transition.Data.Conditions.Add(condition);
            step.Data.Behaviors.Data.Behaviors.Add(behavior);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When you fast-forward and activate it,
            step.LifeCycle.MarkToFastForward();
            step.LifeCycle.Activate();

            // Then everything is completed.
            Assert.AreEqual(Stage.Active, step.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActive()
        {
            // Given a step,
            IBehavior behavior = new EndlessBehaviorMock();
            ICondition condition = new EndlessConditionMock();
            Transition transition = new Transition();
            IStep step = new Step("Step");
            transition.Data.Conditions.Add(condition);
            step.Data.Behaviors.Data.Behaviors.Add(behavior);
            step.Data.Transitions.Data.Transitions.Add(transition);
            step.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            step.LifeCycle.Activate();

            // When you fast-forward it,
            step.LifeCycle.MarkToFastForward();

            // Then everything is completed.
            Assert.AreEqual(Stage.Active, step.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, condition.LifeCycle.Stage);

            yield break;
        }
    }
}
