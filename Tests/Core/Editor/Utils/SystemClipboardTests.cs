// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

//namespace VRBuilder.Editor.Tests
//{
//    public class SystemClipboardTests
//    {
//        [SetUp]
//        public void Setup()
//        {
//            if (RuntimeConfigurator.Exists)
//            {
//                return;
//            }

//            ProcessSceneSetup.Run();
//        }

//        [Test]
//        public void CopyIntoBuffer()
//        {
//            // Given a step and a value in the system copy buffer
//            IStep step = new Step("Step");
//            string testValue = "Hello!";
//            EditorGUIUtility.systemCopyBuffer = testValue;

//            // When I copy that step
//            SystemClipboard.CopyStep(step);

//            // Then the system's copy buffer value has changed.

//            Assert.AreNotEqual(testValue, EditorGUIUtility.systemCopyBuffer);
//        }

//        [Test]
//        public void PasteNewInstance()
//        {
//            // Given a step
//            IStep step = new Step("Step");

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then it creates a new instance of the step.
//            Assert.IsFalse(ReferenceEquals(step, copy));
//        }

//        [Test]
//        public void PasteMultipleTimes()
//        {
//            // Given a step
//            IStep step = new Step("Step");

//            // When I copy and then paste it multiple times
//            SystemClipboard.CopyStep(step);
//            IStep copy1 = SystemClipboard.PasteStep();
//            IStep copy2 = SystemClipboard.PasteStep();
//            IStep copy3 = SystemClipboard.PasteStep();

//            // Then it creates multiple new instances.
//            Assert.NotNull(copy1);
//            Assert.NotNull(copy2);
//            Assert.NotNull(copy3);
//        }

//        [Test]
//        public void PasteWithName()
//        {
//            // Given a step with a name
//            IStep step = new Step("Quite rare name");

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy has the same name
//            Assert.AreEqual(step.Data.Name, copy.Data.Name);
//        }

//        [Test]
//        public void PasteWithBehavior()
//        {
//            // Given a step with a behavior
//            IStep step = new BasicStepBuilder("Step")
//                .AddBehavior(new EmptyBehaviorMock())
//                .Build();

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // The copy has a new copy of the behavior
//            Assert.IsFalse(ReferenceEquals(
//                step.Data.Behaviors.Data.Behaviors.First(),
//                copy.Data.Behaviors.Data.Behaviors.First()));
//        }

//        [Test]
//        public void PasteKeepBehaviorType()
//        {
//            // Given a step with a behavior
//            IStep step = new BasicStepBuilder("Step")
//                .AddBehavior(new EmptyBehaviorMock())
//                .Build();

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // The copy of the behavior has the same type.
//            Assert.AreEqual(
//                step.Data.Behaviors.Data.Behaviors.First().GetType(),
//                copy.Data.Behaviors.Data.Behaviors.First().GetType());
//        }

//        [Test]
//        public void PasteWithPrimitiveValue()
//        {
//            // Given a step with a behavior with a primitive value
//            IStep step = new BasicStepBuilder("Step")
//                .AddBehavior(new ValueBehaviorMock(5f))
//                .Build();

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy stores the same value as the original behavior.
//            Assert.AreEqual(
//                ((ValueBehaviorMock)step.Data.Behaviors.Data.Behaviors.First()).Data.Value,
//                ((ValueBehaviorMock)copy.Data.Behaviors.Data.Behaviors.First()).Data.Value);
//        }

//        [Test]
//        public void PasteWithPropertyReference()
//        {
//            // Given a step with a behavior with a property reference
//            IStep step = new BasicStepBuilder("Step")
//                .AddBehavior(new ObjectReferenceBehaviorMock("Quite unique name"))
//                .Build();

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy stores the same value as the original behavior.
//            Assert.AreEqual(
//                ((ObjectReferenceBehaviorMock)step.Data.Behaviors.Data.Behaviors.First()).Data.ReferenceObject.UniqueName,
//                ((ObjectReferenceBehaviorMock)copy.Data.Behaviors.Data.Behaviors.First()).Data.ReferenceObject.UniqueName);
//        }

//        [Test]
//        public void PasteWithTransition()
//        {
//            // Given a step with a transition to another step,
//            IStep step = new Step("Step");
//            IStep target = new Step("Step 2");

//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.First().Data.TargetStep = target;

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy's transition leads to the end of chapter.
//            Assert.IsNull(copy.Data.Transitions.Data.Transitions.First().Data.TargetStep);
//        }

//        [Test]
//        public void PasteWithTransitionToItself()
//        {
//            // Given a step with a transition to itself,
//            IStep step = new Step("Step");

//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.First().Data.TargetStep = step;

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy's transition leads to the end of chapter.
//            Assert.IsNull(copy.Data.Transitions.Data.Transitions.First().Data.TargetStep);
//        }

//        [Test]
//        public void PasteWithCondition()
//        {
//            // Given a step with a transition with a condition,
//            IStep step = new BasicStepBuilder("Step")
//                .AddCondition(new EndlessConditionMock())
//                .Build();
//            IStep target = new Step("Step 2");
//            step.Data.Transitions.Data.Transitions.First().Data.TargetStep = target;

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy's transition has a copy of the condition.
//            Assert.AreEqual(
//                step.Data.Transitions.Data.Transitions.First().Data.Conditions.First().GetType(),
//                copy.Data.Transitions.Data.Transitions.First().Data.Conditions.First().GetType());

//            Assert.IsFalse(ReferenceEquals(
//                step.Data.Transitions.Data.Transitions.First().Data.Conditions.First(),
//                copy.Data.Transitions.Data.Transitions.First().Data.Conditions.First()));
//        }

//        [Test]
//        public void PasteMultipleTransitions()
//        {
//            // Given a step with three transitions,
//            IStep step = new Step("Step");

//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.Add(new Transition());

//            // When I copy and paste it,
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy has three transitions, too
//            Assert.AreEqual(3, copy.Data.Transitions.Data.Transitions.Count);
//        }


//        [Test]
//        public void PasteMultipleTransitionsAndConditions()
//        {
//            // Given a step with three transitions with different conditions,
//            IStep step = new Step("Step");

//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.Add(new Transition());

//            step.Data.Transitions.Data.Transitions[0].Data.Conditions.Add(new EndlessConditionMock());
//            step.Data.Transitions.Data.Transitions[1].Data.Conditions.Add(new OptionalEndlessConditionMock());
//            step.Data.Transitions.Data.Transitions[2].Data.Conditions.Add(new AutoCompletedCondition());

//            // When I copy and paste it,
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the copy has three transitions, too
//            Assert.AreEqual(typeof(EndlessConditionMock), copy.Data.Transitions.Data.Transitions[0].Data.Conditions[0].GetType());
//            Assert.AreEqual(typeof(OptionalEndlessConditionMock), copy.Data.Transitions.Data.Transitions[1].Data.Conditions[0].GetType());
//            Assert.AreEqual(typeof(AutoCompletedCondition), copy.Data.Transitions.Data.Transitions[2].Data.Conditions[0].GetType());
//        }

//        [Test]
//        public void PasteWithTransitionsWithTargets()
//        {
//            // Given a step with three transitions to different steps,
//            IStep step = new Step("Step");
//            IStep target1 = new Step("Target 1");
//            IStep target2 = new Step("Target 2");
//            IStep target3 = new Step("Target 3");

//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions.Add(new Transition());
//            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = target1;
//            step.Data.Transitions.Data.Transitions[1].Data.TargetStep = target2;
//            step.Data.Transitions.Data.Transitions[2].Data.TargetStep = target3;

//            // When I copy and paste it
//            SystemClipboard.CopyStep(step);
//            IStep copy = SystemClipboard.PasteStep();

//            // Then the all copy's transitions leads to the end of chapter.
//            Assert.IsNull(copy.Data.Transitions.Data.Transitions[0].Data.TargetStep);
//            Assert.IsNull(copy.Data.Transitions.Data.Transitions[1].Data.TargetStep);
//            Assert.IsNull(copy.Data.Transitions.Data.Transitions[2].Data.TargetStep);
//        }
//    }
//}
