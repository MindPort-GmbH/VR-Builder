// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils.Builders;
using VRBuilder.Core.Tests.Utils.Mocks;

namespace VRBuilder.Core.Tests.Processes
{
    public class ChapterTests : RuntimeTests
    {
        [Test]
        public void SetupTest()
        {
            Chapter chapter = TestLinearChapterBuilder.SetupChapterBuilder().Build();
            // Name should be added to the Chapter
            Assert.AreEqual("Chapter1", chapter.Data.Name);
            // State is correct
            Assert.AreEqual(chapter.LifeCycle.Stage, Stage.Inactive, "Chapter should not be active");
            // Assert that FirstStep is set
            Assert.IsNotNull(chapter.Data.FirstStep, "FirstStep is not null.");
            // Assert that CurrentStep is null
            Assert.IsNull(chapter.Data.Current, "Current is null.");
        }

        [Test]
        public void FirstStepIsSet()
        {
            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder();

            Chapter chapter = builder.Build();

            Assert.IsNotNull(chapter.Data.FirstStep, "First step not set!");
            Assert.AreEqual(builder.Steps.First(), chapter.Data.FirstStep, "Wrong Step set as first!");
        }

        [Test]
        public void DeactivateWhileNotActive()
        {
            // Setup Chapter
            Chapter chapter = TestLinearChapterBuilder.SetupChapterBuilder().Build();

            bool didNotFail = false;
            bool isWrongException = false;

            // Expect to fail on calling Deactivate() before activating the Chapter.
            try
            {
                chapter.LifeCycle.Deactivate();
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

            // Check if exception was thrown.
            Assert.IsFalse(didNotFail, "No Exception was raised!");
            Assert.IsFalse(isWrongException, "Wrong Exception was raised!");
        }

        [UnityTest]
        public IEnumerator ActivationIsDone()
        {
            // Setup Chapter
            Chapter chapter = TestLinearChapterBuilder.SetupChapterBuilder(1, false).Build();

            // Activate should work on simple steps.
            ProcessRunner.Initialize(new Process("Process", chapter));
            ProcessRunner.Run();

            while (chapter.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
            }

            // Chapter should be finished now.
            Assert.AreEqual(Stage.Inactive, chapter.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator FirstStepGetActivated()
        {
            // Setup Chapter
            Chapter chapter = TestLinearChapterBuilder.SetupChapterBuilder(3, true).Build();
            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // Activate should work on simple steps.
            chapter.LifeCycle.Activate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            // First Step should be active now.
            Assert.IsNotNull(chapter.Data.Current, "First step not set");
            Assert.AreEqual(chapter.Data.Current.LifeCycle.Stage, Stage.Active, "First Step of Chapter was not activated");
        }

        [UnityTest]
        public IEnumerator MultiStepChapterCompletesSteps()
        {
            // Setup Chapter
            TestLinearChapterBuilder builder = TestLinearChapterBuilder.SetupChapterBuilder(2, true);
            Chapter chapter = builder.Build();

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // Activate should work on simple steps.
            chapter.LifeCycle.Activate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage chapterInitalStage = chapter.LifeCycle.Stage;
            Stage step1InitialStage = builder.Steps[0].LifeCycle.Stage;
            Stage step2InitialStage = builder.Steps[1].LifeCycle.Stage;

            builder.StepTriggerConditions[0].Autocomplete();

            while (chapter.Data.Steps[1].LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage chapterStageAfterFirstComplete = chapter.LifeCycle.Stage;
            Stage step1StageAfterFirstComplete = builder.Steps[0].LifeCycle.Stage;
            Stage step2StageAfterFirstComplete = builder.Steps[1].LifeCycle.Stage;

            builder.StepTriggerConditions[1].Autocomplete();

            while (chapter.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage chapterStageInEnd = chapter.LifeCycle.Stage;
            Stage step1StageInEnd = builder.Steps[0].LifeCycle.Stage;
            Stage step2StageInEnd = builder.Steps[1].LifeCycle.Stage;

            // check steps are activated and completed in correct order
            Assert.AreEqual(Stage.Activating, chapterInitalStage, "Chapter should be active in the beginning");
            Assert.AreEqual(Stage.Active, step1InitialStage, "First Step should be active in the beginning");
            Assert.AreEqual(Stage.Inactive, step2InitialStage, "Second Step should not be active in the beginning");

            Assert.AreEqual(Stage.Activating, chapterStageAfterFirstComplete, "Chapter should be active after first complete");
            Assert.AreEqual(Stage.Inactive, step1StageAfterFirstComplete, "First Step should not be active after first complete");
            Assert.AreEqual(Stage.Active, step2StageAfterFirstComplete, "Second Step should be active after first complete");

            Assert.AreEqual(Stage.Active, chapterStageInEnd, "Chapter should still be active, as nothing deactivated it.");
            Assert.AreEqual(Stage.Inactive, step1StageInEnd, "First Step should not be active in the end");
            Assert.AreEqual(Stage.Inactive, step2StageInEnd, "Second Step should not be active in the end");
        }

        [UnityTest]
        public IEnumerator LoopingStepsActivationStates()
        {
            // Given a chapter with two steps that have transitions with one condition to each other (a "loop"),
            Step step1 = new Step("First");
            Step step2 = new Step("Second");

            Chapter chapter = new Chapter("Looping Chapter", step1);
            chapter.Data.Steps.Add(step2);

            Transition transition1 = new Transition();
            Transition transition2 = new Transition();
            transition1.Data.TargetStep = step2;
            transition2.Data.TargetStep = step1;

            EndlessConditionMock condition1 = new EndlessConditionMock();
            EndlessConditionMock condition2 = new EndlessConditionMock();
            transition1.Data.Conditions.Add(condition1);
            transition2.Data.Conditions.Add(condition2);

            step1.Data.Transitions.Data.Transitions.Add(transition1);
            step2.Data.Transitions.Data.Transitions.Add(transition2);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter and complete every condition,
            chapter.LifeCycle.Activate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage chapterInitalStage = chapter.LifeCycle.Stage;
            Stage step1InitialStage = step1.LifeCycle.Stage;
            Stage step2InitialStage = step2.LifeCycle.Stage;

            condition1.Autocomplete();

            while (step2.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage chapterStageAfterFirstComplete = chapter.LifeCycle.Stage;
            Stage step1StageAfterFirstComplete = step1.LifeCycle.Stage;
            Stage step2StageAfterFirstComplete = step2.LifeCycle.Stage;

            condition2.Autocomplete();

            while (step1.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            Stage chapterStageAfterSecondComplete = chapter.LifeCycle.Stage;
            Stage step1StageAfterSecondComplete = step1.LifeCycle.Stage;
            Stage step2StageAfterSecondComplete = step2.LifeCycle.Stage;

            // Then the first step is active again and every ActivationState after each condition completion has been correct.
            Assert.AreEqual(Stage.Activating, chapterInitalStage, "Chapter should be activating in the beginning");
            Assert.AreEqual(Stage.Active, step1InitialStage, "First Step should be active in the beginning");
            Assert.AreEqual(Stage.Inactive, step2InitialStage, "Second Step should be inactive in the beginning");

            Assert.AreEqual(Stage.Activating, chapterStageAfterFirstComplete, "Chapter should be activating after first complete");
            Assert.AreEqual(Stage.Inactive, step1StageAfterFirstComplete, "First Step should be deactivated after first complete");
            Assert.AreEqual(Stage.Active, step2StageAfterFirstComplete, "Second Step should be active after first complete");

            Assert.AreEqual(Stage.Activating, chapterStageAfterSecondComplete, "Chapter should not be activating after second complete because of the loop");
            Assert.AreEqual(Stage.Active, step1StageAfterSecondComplete, "First Step should be active after second complete because of the loop");
            Assert.AreEqual(Stage.Inactive, step2StageAfterSecondComplete, "Second Step should be deactivated after second complete because of the loop");

            yield break;
        }

        [UnityTest]
        public IEnumerator LoopingStepsWithOneEndStep()
        {
            // Given a chapter with three steps
            // where the first two steps are connected to each other with two transitions with each one condition ("loop")
            // and the second step is connected to the third step with a third transition with one condition,
            Step step1 = new Step("First");
            Step step2 = new Step("Second");
            Step step3 = new Step("Third");

            Chapter chapter = new Chapter("Chapter 1", step1);
            chapter.Data.Steps.Add(step2);
            chapter.Data.Steps.Add(step3);

            Transition transition1 = new Transition();
            Transition transition2 = new Transition();
            Transition transition3 = new Transition();
            Transition transitionToEnd = new Transition();
            transition1.Data.TargetStep = step2;
            transition2.Data.TargetStep = step1;
            transition3.Data.TargetStep = step3;

            EndlessConditionMock condition1 = new EndlessConditionMock();
            EndlessConditionMock condition2 = new EndlessConditionMock();
            EndlessConditionMock condition3 = new EndlessConditionMock();
            transition1.Data.Conditions.Add(condition1);
            transition2.Data.Conditions.Add(condition2);
            transition3.Data.Conditions.Add(condition3);

            step1.Data.Transitions.Data.Transitions.Add(transition1);
            step2.Data.Transitions.Data.Transitions.Add(transition2);
            step2.Data.Transitions.Data.Transitions.Add(transition3);
            step3.Data.Transitions.Data.Transitions.Add(transitionToEnd);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate the chapter and complete the third condition after looping the first two steps once,
            chapter.LifeCycle.Activate();

            while (chapter.Data.FirstStep.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            condition1.Autocomplete();

            while (step2.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            condition2.Autocomplete();

            while (step1.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            condition1.Autocomplete();

            while (step2.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            condition3.Autocomplete();

            while (chapter.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                chapter.Update();
            }

            // Then the chapter and each step are deactivated.
            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage, "Chapter should be active in the end");
            Assert.AreEqual(Stage.Inactive, step1.LifeCycle.Stage, "Step1 should not be active in the end");
            Assert.AreEqual(Stage.Inactive, step2.LifeCycle.Stage, "Step2 should not be active in the end");
            Assert.AreEqual(Stage.Inactive, step3.LifeCycle.Stage, "Step3 should not be active in the end");

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveChapter()
        {
            // Given a chapter,
            Chapter chapter = new LinearChapterBuilder("Chapter")
                .AddStep(new BasicStepBuilder("Step")
                    .AddCondition(new EndlessConditionMock()))
                .Build();

            // When it's marked to be fast-forwarded,
            chapter.LifeCycle.MarkToFastForward();

            yield return null;
            chapter.Update();

            // Then it doesn't start its activation on its own.
            Assert.AreEqual(Stage.Inactive, chapter.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveChapterAndThenActivate()
        {
            // Given a chapter,
            Chapter chapter = new LinearChapterBuilder("Chapter")
                .AddStep(new BasicStepBuilder("Step")
                    .AddCondition(new EndlessConditionMock()))
                .Build();

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When it's marked to be fast-forwarded,
            chapter.LifeCycle.MarkToFastForward();
            chapter.LifeCycle.Activate();

            // Then it's activated.
            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);

            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActiveChapter()
        {
            // Given an activated chapter,
            Chapter chapter = new LinearChapterBuilder("Chapter")
                .AddStep(new BasicStepBuilder("Step")
                    .AddCondition(new EndlessConditionMock()))
                .Build();

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            chapter.LifeCycle.Activate();

            // When it's marked to be fast-forwarded,
            chapter.LifeCycle.MarkToFastForward();

            yield return null;
            chapter.Update();

            // Then it's activated.
            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator FastForwardBranchingChapter()
        {
            // Given a chapter with a step branching to three transitions,
            Step branchingStep = new Step("Branching Step");
            Chapter chapter = new Chapter("Chapter", branchingStep);
            Transition firstTransition = new Transition();
            Transition secondTransition = new Transition();
            Transition thirdTransition = new Transition();
            EndlessConditionMock firstConditionMock = new EndlessConditionMock();
            EndlessConditionMock secondConditionMock = new EndlessConditionMock();
            EndlessConditionMock thirdConditionMock = new EndlessConditionMock();
            firstTransition.Data.Conditions.Add(firstConditionMock);
            secondTransition.Data.Conditions.Add(secondConditionMock);
            thirdTransition.Data.Conditions.Add(thirdConditionMock);
            branchingStep.Data.Transitions.Data.Transitions.Add(firstTransition);
            branchingStep.Data.Transitions.Data.Transitions.Add(secondTransition);
            branchingStep.Data.Transitions.Data.Transitions.Add(thirdTransition);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            chapter.LifeCycle.Activate();

            yield return null;
            chapter.Update();

            // When it's marked to be fast-forwarded,
            chapter.LifeCycle.MarkToFastForward();

            // Then only the first transition completes.
            Assert.IsTrue(firstTransition.IsCompleted);
            Assert.IsFalse(secondTransition.IsCompleted);
            Assert.IsFalse(thirdTransition.IsCompleted);
        }

        [UnityTest]
        public IEnumerator FastForwardLoopingChapter()
        {
            // Given a chapter with a looping step,
            Step loopingStep = new Step("Looping Step");
            Chapter chapter = new Chapter("Chapter", loopingStep);
            Transition loopingTransition = new Transition();
            loopingTransition.Data.TargetStep = loopingStep;
            Transition endTransition = new Transition();
            EndlessConditionMock conditionMock = new EndlessConditionMock();
            endTransition.Data.Conditions.Add(conditionMock);
            loopingStep.Data.Transitions.Data.Transitions.Add(loopingTransition);
            loopingStep.Data.Transitions.Data.Transitions.Add(endTransition);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            chapter.LifeCycle.Activate();

            int loops = 0;
            while (loops < 2)
            {
                while (loopingStep.LifeCycle.Stage != Stage.Active)
                {
                    Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                    yield return null;
                    chapter.Update();
                }


                while (loopingStep.LifeCycle.Stage != Stage.Inactive)
                {
                    Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                    yield return null;
                    chapter.Update();
                }

                loops++;
            }

            // When it's marked to be fast-forwarded,
            chapter.LifeCycle.MarkToFastForward();

            // Then it completes.
            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, loopingStep.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator FastForwardTwoStepsLoopingChapter()
        {
            // Given a chapter with two steps looping between each other,
            Step firstStep = new Step("First Step");
            Step secondStep = new Step("Second Step");
            Chapter chapter = new Chapter("Chapter", firstStep);
            chapter.Data.Steps.Add(secondStep);
            Transition firstToSecond = new Transition();
            firstToSecond.Data.TargetStep = secondStep;
            Transition secondToFirst = new Transition();
            secondToFirst.Data.TargetStep = firstStep;
            Transition secondToEnd = new Transition();
            EndlessConditionMock conditionMock = new EndlessConditionMock();
            secondToEnd.Data.Conditions.Add(conditionMock);
            firstStep.Data.Transitions.Data.Transitions.Add(firstToSecond);
            secondStep.Data.Transitions.Data.Transitions.Add(secondToFirst);
            secondStep.Data.Transitions.Data.Transitions.Add(secondToEnd);

            chapter.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            chapter.LifeCycle.Activate();

            int loops = 0;
            while (loops < 2)
            {
                while (firstStep.LifeCycle.Stage != Stage.Active)
                {
                    Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                    Assert.AreEqual(Stage.Inactive, secondStep.LifeCycle.Stage);
                    yield return null;
                    chapter.Update();
                }


                while (firstStep.LifeCycle.Stage != Stage.Inactive)
                {
                    Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                    Assert.AreEqual(Stage.Inactive, secondStep.LifeCycle.Stage);
                    yield return null;
                    chapter.Update();
                }

                while (secondStep.LifeCycle.Stage != Stage.Active)
                {
                    Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                    Assert.AreEqual(Stage.Inactive, firstStep.LifeCycle.Stage);
                    yield return null;
                    chapter.Update();
                }

                while (secondStep.LifeCycle.Stage != Stage.Inactive)
                {
                    Assert.AreEqual(Stage.Activating, chapter.LifeCycle.Stage);
                    Assert.AreEqual(Stage.Inactive, firstStep.LifeCycle.Stage);
                    yield return null;
                    chapter.Update();
                }

                loops++;
            }

            // When it's marked to be fast-forwarded,
            chapter.LifeCycle.MarkToFastForward();

            // Then it completes.
            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, firstStep.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, secondStep.LifeCycle.Stage);
        }
    }
}
