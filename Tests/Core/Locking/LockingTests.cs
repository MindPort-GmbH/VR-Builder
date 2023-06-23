// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core;
using VRBuilder.Core.Properties;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Tests.Builder;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using NUnit.Framework;
using UnityEngine.TestTools;
using VRBuilder.Core.Configuration;

namespace VRBuilder.Tests.Locking
{
    public class LockingTests : RuntimeTests
    {
        public readonly DefaultStepLockHandling LockHandling = new DefaultStepLockHandling();

        [UnityTest]
        public IEnumerator LockAtEndPropertyIsLockedAfterFinishingStep()
        {
            // Given a transition with a condition referencing a scene object with a lockable property
            // that should be locked in the end of a step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock lockAtEndOfStepCondition = new LockableReferencingConditionMock();
            lockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            lockAtEndOfStepCondition.LockableProperties = new[] { new LockablePropertyData(property, true) };

            Step step2 = new BasicStepBuilder("step2").Build();

            Step step = new BasicStepBuilder("step").AddCondition(lockAtEndOfStepCondition).Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine and completing the step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            // Then the property is locked in the end.
            Assert.IsTrue(property.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator KeepUnlockedAtEndPropertyIsUnlockedAfterFinishingStep()
        {
            // Given a transition with a condition referencing a scene object with a lockable property
            // that should not be locked in the end of a step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock doNotLockAtEndOfStepCondition = new LockableReferencingConditionMock();
            doNotLockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            doNotLockAtEndOfStepCondition.LockableProperties = new[] { new LockablePropertyData(property, false) };

            Step step2 = new BasicStepBuilder("step2").Build();

            Step step = new BasicStepBuilder("step").AddCondition(doNotLockAtEndOfStepCondition).Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine and completing the step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            // Then the property is not locked in the end.
            Assert.IsFalse(property.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SecondPropertyStaysLocked()
        {
            // Given two steps and two scene objects with each one lockable property
            // where only the first property is referenced in the first step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property1 = o1.GameObject.AddComponent<LockablePropertyMock>();

            ISceneObject o2 = TestingUtils.CreateSceneObject("o2");
            LockablePropertyMock property2 = o2.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock doNotLockAtEndOfStepCondition = new LockableReferencingConditionMock();
            doNotLockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            doNotLockAtEndOfStepCondition.LockableProperties = new[] {new LockablePropertyData(property1, false)};

            Step step2 = new BasicStepBuilder("step2").Build();
            Step step = new BasicStepBuilder("step")
                .AddCondition(doNotLockAtEndOfStepCondition)
                .Build();

            property1.SetLocked(true);
            property2.SetLocked(true);

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine in the first step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());

            // Then property 2 is locked during the first step.
            Assert.IsTrue(property2.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetLockablePropertiesInTransition()
        {
            // Given a scene object with one lockable property and one non-lockable property used by a condition in a transition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();
            o1.GameObject.AddComponent<PropertyMock>();

            LockableReferencingConditionMock condition = new LockableReferencingConditionMock();
            condition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            condition.LockableProperties = new[] { new LockablePropertyData(property, true) };

            Step step2 = new BasicStepBuilder("step2").Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition).Build();
            Transition transition = (Transition)step.Data.Transitions.Data.Transitions[0];
            transition.Data.TargetStep = step2;

            // When counting the lockable properties in the transition.
            // Then there is exactly one lockable property.
            Assert.IsTrue(transition.GetLockableProperties().Count() == 1);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetLockablePropertiesZeroLockables()
        {
            // Given a scene object with one non-lockable property used by a condition in a transition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            o1.GameObject.AddComponent<PropertyMock>();

            ReferencingConditionMock condition = new ReferencingConditionMock();
            condition.Data.PropertyMock = new ScenePropertyReference<PropertyMock>(o1.UniqueName);

            Step step2 = new BasicStepBuilder("step2").Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition).Build();
            Transition transition = (Transition)step.Data.Transitions.Data.Transitions[0];
            transition.Data.TargetStep = step2;

            // When counting the lockable properties in the transition.
            // Then there is no lockable property.
            Assert.IsFalse(transition.GetLockableProperties().Any());

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetLockablePropertiesObjectInMultipleConditions()
        {
            // Given a scene object with one lockable property and one non-lockable property used by two conditions in a transition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();
            o1.GameObject.AddComponent<PropertyMock>();

            LockableReferencingConditionMock condition1 = new LockableReferencingConditionMock();
            condition1.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            condition1.LockableProperties = new[] { new LockablePropertyData(property, true) };

            LockableReferencingConditionMock condition2 = new LockableReferencingConditionMock();
            condition2.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            condition2.LockableProperties = new[] { new LockablePropertyData(property, false) };

            Step step2 = new BasicStepBuilder("step2").Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition1).AddCondition(condition2).Build();

            Transition transition = (Transition)step.Data.Transitions.Data.Transitions[0];
            transition.Data.TargetStep = step2;

            // When counting the lockable properties in the transition.
            // Then there is exactly one lockable property.
            Assert.IsTrue(transition.GetLockableProperties().Count() == 1);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetLockablePropertiesMultipleObjectsAndConditions()
        {
            // Given three scene objects,
            // one of them with one lockable property and one non-lockable property,
            // one of them with only one non-lockable property,
            // and one of them with only one lockable property,
            // used by three different conditions in a transition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();
            o1.GameObject.AddComponent<PropertyMock>();

            ISceneObject o2 = TestingUtils.CreateSceneObject("o2");
            o2.GameObject.AddComponent<PropertyMock>();

            ISceneObject o3 = TestingUtils.CreateSceneObject("o3");
            LockablePropertyMock property2 = o2.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock condition1 = new LockableReferencingConditionMock();
            condition1.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            condition1.LockableProperties = new[] { new LockablePropertyData(property, true) };

            ReferencingConditionMock condition2 = new ReferencingConditionMock();
            condition2.Data.PropertyMock = new ScenePropertyReference<PropertyMock>(o2.UniqueName);

            LockableReferencingConditionMock condition3 = new LockableReferencingConditionMock();
            condition3.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o3.UniqueName);
            condition3.LockableProperties = new[] { new LockablePropertyData(property2, true) };

            Step step2 = new BasicStepBuilder("step2").Build();
            Step step = new BasicStepBuilder("step")
                .AddCondition(condition1)
                .AddCondition(condition2)
                .AddCondition(condition3)
                .Build();
            Transition transition = (Transition)step.Data.Transitions.Data.Transitions[0];
            transition.Data.TargetStep = step2;

            // When counting the lockable properties in the transition.
            // Then there are exactly two lockable properties.
            Assert.IsTrue(transition.GetLockableProperties().Count() == 2);

            yield return null;
        }

        [UnityTest]
        public IEnumerator EmptyTransition()
        {
            // Given a transition from one step to another without any conditions.
            Step step2 = new BasicStepBuilder("step2").Build();
            Step step = new BasicStepBuilder("step").Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine in the first step.
            try
            {
                LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
                LockHandling.Lock(step.Data, new List<LockablePropertyData>());
            }
            // Then no exception is thrown.
            catch (Exception exception)
            {
                Assert.Fail("Expected no exception, but got: " + exception.Message);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SamePropertyInSecondStepStaysUnlocked()
        {
            // Given a scene object with a lockable property which is referenced in the transitions of the first and second step
            // and should normally lock at the end of a step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock lockAtEndOfStepCondition = new LockableReferencingConditionMock();
            lockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            lockAtEndOfStepCondition.LockableProperties = new[] { new LockablePropertyData(property, true) };

            Step step2 = new BasicStepBuilder("step2").AddCondition(lockAtEndOfStepCondition).Build();

            Step step = new BasicStepBuilder("step").AddCondition(lockAtEndOfStepCondition).Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine in the first step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            // Then the property is not locked at the end of the first step because it is needed in the second step.
            Assert.IsFalse(property.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SamePropertyInSecondStepGetsLockedAfterSecondStep()
        {
            // Given three steps and
            // a scene object with a lockable property that should normally lock at the end of a step.
            // It is referenced in the transitions of the first and second step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock lockAtEndOfStepCondition = new LockableReferencingConditionMock();
            lockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            lockAtEndOfStepCondition.LockableProperties = new[] { new LockablePropertyData(property, true) };

            Step step3 = new BasicStepBuilder("step3").AddCondition(new EndlessConditionMock()).Build();
            Step step2 = new BasicStepBuilder("step2").AddCondition(lockAtEndOfStepCondition).Build();
            step2.Data.Transitions.Data.Transitions[0].Data.TargetStep = step3;

            Step step = new BasicStepBuilder("step").AddCondition(lockAtEndOfStepCondition).Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine in the first and second step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            LockHandling.Unlock(step2.Data, new List<LockablePropertyData>());
            step2.Data.Transitions.Data.Transitions.First().Autocomplete();
            step2.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step2.Data, new List<LockablePropertyData>());

            // Then the property is locked at the end of the second step because it is not needed any further.
            Assert.IsTrue(property.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SecondPropertyGetsLockedAfterNotBeingNeededAnymore()
        {
            // Given two steps and two scene objects with each one lockable property
            // and property 2 (lock at the end of step = true) is referenced only in the first step,
            // whereas property 1 (lock at the end of step = false) is referenced in both steps.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock DoNotLockAtEndOfStep = o1.GameObject.AddComponent<LockablePropertyMock>();

            ISceneObject o2 = TestingUtils.CreateSceneObject("o2");
            LockablePropertyMock lockAtEndOfStep = o2.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock doNotLockAtEndOfStepCondition = new LockableReferencingConditionMock();
            doNotLockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            doNotLockAtEndOfStepCondition.LockableProperties = new[] {new LockablePropertyData(DoNotLockAtEndOfStep, false)};

            LockableReferencingConditionMock lockAtEndOfStepCondition = new LockableReferencingConditionMock();
            lockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o2.UniqueName);
            lockAtEndOfStepCondition.LockableProperties = new[] {new LockablePropertyData(lockAtEndOfStep, true)};

            Step step2 = new BasicStepBuilder("step2").AddCondition(doNotLockAtEndOfStepCondition).Build();
            Step step = new BasicStepBuilder("step")
                .AddCondition(doNotLockAtEndOfStepCondition)
                .AddCondition(lockAtEndOfStepCondition)
                .Build();

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine in the first step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            // Then property 2 is locked in the end of the first step.
            Assert.IsTrue(lockAtEndOfStep.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SecondTransitionIsLocked()
        {
            // Given a step with two different transitions,
            // each of them having an own condition with a lockable property which should not be locked at the end of a step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            ISceneObject o2 = TestingUtils.CreateSceneObject("o2");
            LockablePropertyMock property2 = o2.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock doNotLockAtEndOfStepCondition1 = new LockableReferencingConditionMock();
            doNotLockAtEndOfStepCondition1.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            doNotLockAtEndOfStepCondition1.LockableProperties = new[] {new LockablePropertyData(property, false)};

            LockableReferencingConditionMock doNotLockAtEndOfStepCondition2 = new LockableReferencingConditionMock();
            doNotLockAtEndOfStepCondition2.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o2.UniqueName);
            doNotLockAtEndOfStepCondition2.LockableProperties = new[] {new LockablePropertyData(property2, false)};

            Step step2 = new BasicStepBuilder("step2").AddCondition(doNotLockAtEndOfStepCondition1).Build();

            Step step = new BasicStepBuilder("step").AddCondition(doNotLockAtEndOfStepCondition1).Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;
            step.Data.Transitions.Data.Transitions.Add(new Transition());
            step.Data.Transitions.Data.Transitions[1].Data.Conditions.Add(doNotLockAtEndOfStepCondition2);

            // When completing only the first transition.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            // Then the lockable property of the second transition is locked,
            // even though lock at the end of step is set to false.
            Assert.IsTrue(property2.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnlockedAfterManualUnlockStarted()
        {
            // Given a scene object with a lockable property which is not referenced by any condition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            EndlessConditionMock condition = new EndlessConditionMock();

            Step step2 = new BasicStepBuilder("step").AddCondition(new EndlessConditionMock()).Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition).Build();

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            property.SetLocked(true);

            // When we include the property into the manualUnlocked list of the UnlockPropertiesForStepData method.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(property)});

            // Then the property is not locked.
            Assert.IsFalse(property.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator OtherPropertyNotUnlockedAfterManualUnlockStarted()
        {
            // Given a scene object with two independent lockable properties which are not referenced by any condition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockablePropertyMock property2 = o1.GameObject.AddComponent<LockablePropertyMock>();

            EndlessConditionMock condition = new EndlessConditionMock();

            Step step2 = new BasicStepBuilder("step").AddCondition(new EndlessConditionMock()).Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition).Build();

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            property.SetLocked(true);
            property2.SetLocked(true);

            // When we manually unlock one property.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(property)});

            // Then the other stays is locked.
            Assert.IsTrue(property2.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator LockedAfterManualUnlockFinished()
        {
            // Given a scene object with a lockable property which is not referenced by any condition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            EndlessConditionMock condition = new EndlessConditionMock();

            Step step2 = new BasicStepBuilder("step").AddCondition(new EndlessConditionMock()).Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition).Build();

            property.SetLocked(true);
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When we manually unlock the property for one step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(property)});

            // Then it is locked again after the step was completed.
            Assert.IsTrue(property.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator OtherPropertyLockedAfterManualUnlockFinished()
        {
            // Given a scene object with two independent lockable properties which are not referenced by any condition.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockablePropertyMock property2 = o1.GameObject.AddComponent<LockablePropertyMock>();

            EndlessConditionMock condition = new EndlessConditionMock();

            Step step2 = new BasicStepBuilder("step").AddCondition(new EndlessConditionMock()).Build();
            Step step = new BasicStepBuilder("step").AddCondition(condition).Build();

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            property.SetLocked(true);
            property2.SetLocked(true);

            // When we manually unlock one property for one step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(property)});

            // Then the other property stays locked after the step was completed.
            Assert.IsTrue(property2.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator DependencyGetsUnlocked()
        {
            // Given a scene object with one lockable property with dependency.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock dependency = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockablePropertyMockWithDependency propertyWithDependency = o1.GameObject.AddComponent<LockablePropertyMockWithDependency>();

            Step step2 = new BasicStepBuilder("step").Build();
            Step step = new BasicStepBuilder("step").AddCondition(new EndlessConditionMock()).Build();

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            dependency.SetLocked(true);
            propertyWithDependency.SetLocked(true);

            // When we manually unlock the property for one step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(propertyWithDependency)});

            // Then the dependent property is also unlocked.
            Assert.IsFalse(dependency.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator DependencyGetsLocked()
        {
            // Given a scene object with one lockable property with dependency.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock dependency = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockablePropertyMockWithDependency propertyWithDependency = o1.GameObject.AddComponent<LockablePropertyMockWithDependency>();

            Step step2 = new BasicStepBuilder("step").Build();
            Step step = new BasicStepBuilder("step").AddCondition(new EndlessConditionMock()).Build();

            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            dependency.SetLocked(true);
            propertyWithDependency.SetLocked(true);

            // When we manually unlock the property for one step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(propertyWithDependency)});
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData> {new LockablePropertyData(propertyWithDependency)});

            // Then after the step the dependent property is also locked again.
            Assert.IsTrue(dependency.IsLocked);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectInConditionIsInAutoUnlockList()
        {
            // Given a step with a condition with a LockableProperty
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockableReferencingConditionMock lockCondition = new LockableReferencingConditionMock();
            lockCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            Step step = new BasicStepBuilder("step").AddCondition(lockCondition).Build();

            // When we create a collection referencing this step
            LockableObjectsCollection collection = new LockableObjectsCollection(step.Data);

            // Then the lockable property is in the AutoUnlockList of the collection
            Assert.IsTrue(collection.IsInAutoUnlockList(lockableProperty));

            yield return null;
        }

        [UnityTest]
        public IEnumerator ObjectInConditionIsNotInManualUnlockList()
        {
            // Given a step with a condition with a LockableProperty
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockableReferencingConditionMock lockCondition = new LockableReferencingConditionMock();
            lockCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            Step step = new BasicStepBuilder("step").AddCondition(lockCondition).Build();

            // When we create a collection referencing this step
            LockableObjectsCollection collection = new LockableObjectsCollection(step.Data);

            // Then the lockable property is not in the Manual Unlock List of the collection
            Assert.IsFalse(collection.IsInManualUnlockList(lockableProperty));

            yield return null;
        }

        [UnityTest]
        public IEnumerator ManuallyAddedSceneObjectIsInManualUnlockList()
        {
            // Given a step with a condition with a LockableProperty and a collection referencing this step
            Step step = new BasicStepBuilder("step").Build();
            LockableObjectsCollection collection = new LockableObjectsCollection(step.Data);

            // When we create a SceneObject with a lockable property
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            // ...and add the SceneObject and its property to the collection
            collection.AddSceneObject(o1);
            collection.Add(lockableProperty);

            // Then the lockable property is in the Manual Unlock List of the collection
            Assert.IsTrue(collection.IsInManualUnlockList(lockableProperty));

            yield return null;
        }

        [UnityTest]
        public IEnumerator ManuallyAddedSceneObjectIsNotInAutoUnlockList()
        {
            // Given a step with a condition with a LockableProperty and a collection referencing this step
            Step step = new BasicStepBuilder("step").Build();
            LockableObjectsCollection collection = new LockableObjectsCollection(step.Data);

            // When we create a SceneObject with a lockable property
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            // ...and add the SceneObject and its property to the collection
            collection.AddSceneObject(o1);
            collection.Add(lockableProperty);

            // Then the lockable property is not in the Auto Unlock List of the collection
            Assert.IsFalse(collection.IsInAutoUnlockList(lockableProperty));

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemovedSceneObjectIsNotInManualUnlockList()
        {
            // Given a step with a condition with a LockableProperty
            Step step = new BasicStepBuilder("step").Build();
            // ...and a collection with a manually added SceneObject and an added lockable property
            LockableObjectsCollection collection = new LockableObjectsCollection(step.Data);
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            collection.AddSceneObject(o1);
            collection.Add(lockableProperty);

            // When we remove the SceneObject from the collection
            collection.RemoveSceneObject(o1);

            // Then the lockable property is not in the Auto Unlock List of the collection
            Assert.IsFalse(collection.IsInAutoUnlockList(lockableProperty));

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemovedPropertyIsNotInManualUnlockList()
        {
            // Given a step with a condition with a LockableProperty
            Step step = new BasicStepBuilder("step").Build();
            // ...and a collection with a manually added SceneObject and an added lockable property
            LockableObjectsCollection collection = new LockableObjectsCollection(step.Data);
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            collection.AddSceneObject(o1);
            collection.Add(lockableProperty);

            // When we remove the property from the collection
            collection.Remove(lockableProperty);

            // Then the lockable property is not in the Auto Unlock List of the collection
            Assert.IsFalse(collection.IsInAutoUnlockList(lockableProperty));

            yield return null;
        }

        [UnityTest]
        public IEnumerator PropertyDoesNotLockIfStillRequiredByOtherStep()
        {
            // Given two steps running at the same time which unlock the same property.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockableReferencingConditionMock lockCondition1 = new LockableReferencingConditionMock();
            lockCondition1.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            LockableReferencingConditionMock lockCondition2 = new LockableReferencingConditionMock();
            lockCondition2.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            Step step1 = new BasicStepBuilder("step1").AddCondition(lockCondition1).Build();
            Step step2 = new BasicStepBuilder("step2").AddCondition(lockCondition2).Build();
            step1.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            step2.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When one of the steps completes.
            step1.LifeCycle.Activate();
            step2.LifeCycle.Activate();

            yield return null;
            bool allStepsActivated = false;

            while(allStepsActivated == false)
            {              
                if (step1.LifeCycle.Stage != Stage.Active)
                {
                    step1.Update();
                }

                if(step2.LifeCycle.Stage != Stage.Active)
                {
                    step2.Update();
                }

                yield return null;

                allStepsActivated = step1.LifeCycle.Stage == Stage.Active && step2.LifeCycle.Stage == Stage.Active;
            }

            step1.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            step1.LifeCycle.Deactivate();
            while (step1.LifeCycle.Stage != Stage.Inactive)
            {
                step1.Update();

                yield return null;
            }

            // Then the property stays unlocked.
            Assert.IsFalse(lockableProperty.IsLocked);
        }

        [UnityTest]
        public IEnumerator PropertyLocksAfterNoMoreStepRequireItUnlocked()
        {
            // Given two steps running at the same time which unlock the same property.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockableProperty lockableProperty = o1.GameObject.AddComponent<LockablePropertyMock>();
            LockableReferencingConditionMock lockCondition1 = new LockableReferencingConditionMock();
            lockCondition1.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            LockableReferencingConditionMock lockCondition2 = new LockableReferencingConditionMock();
            lockCondition2.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            Step step1 = new BasicStepBuilder("step1").AddCondition(lockCondition1).Build();
            Step step2 = new BasicStepBuilder("step2").AddCondition(lockCondition2).Build();
            step1.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);
            step2.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When both steps complete.
            step1.LifeCycle.Activate();
            step2.LifeCycle.Activate();

            yield return null;
            bool allStepsActivated = false;

            while (allStepsActivated == false)
            {
                if (step1.LifeCycle.Stage != Stage.Active)
                {
                    step1.Update();
                }

                if (step2.LifeCycle.Stage != Stage.Active)
                {
                    step2.Update();
                }

                yield return null;

                allStepsActivated = step1.LifeCycle.Stage == Stage.Active && step2.LifeCycle.Stage == Stage.Active;
            }

            step1.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            step1.LifeCycle.Deactivate();

            step2.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            step2.LifeCycle.Deactivate();

            bool allStepsDeactivated = false;

            while (allStepsDeactivated == false)
            {
                if (step1.LifeCycle.Stage != Stage.Inactive)
                {
                    step1.Update();
                }

                if (step2.LifeCycle.Stage != Stage.Inactive)
                {
                    step2.Update();
                }

                yield return null;

                allStepsDeactivated = step1.LifeCycle.Stage == Stage.Inactive && step2.LifeCycle.Stage == Stage.Inactive;
            }

            // Then the property is locked.
            Assert.IsTrue(lockableProperty.IsLocked);
        }

        [UnityTest]
        public IEnumerator KeepUnlockedAtEndPropertyCanStillBeLocked()
        {
            // Given a transition with a condition referencing a scene object with a lockable property
            // that should not be locked in the end of a step.
            ISceneObject o1 = TestingUtils.CreateSceneObject("o1");
            LockablePropertyMock property = o1.GameObject.AddComponent<LockablePropertyMock>();

            LockableReferencingConditionMock doNotLockAtEndOfStepCondition = new LockableReferencingConditionMock();
            doNotLockAtEndOfStepCondition.Data.LockablePropertyMock = new ScenePropertyReference<ILockablePropertyMock>(o1.UniqueName);
            doNotLockAtEndOfStepCondition.LockableProperties = new[] { new LockablePropertyData(property, false) };

            Step step2 = new BasicStepBuilder("step2").Build();

            Step step = new BasicStepBuilder("step").AddCondition(doNotLockAtEndOfStepCondition).Build();
            step.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

            // When executing the locking routine and completing the step.
            LockHandling.Unlock(step.Data, new List<LockablePropertyData>());
            step.Data.Transitions.Data.Transitions.First().Autocomplete();
            step.Data.Transitions.Data.Transitions.First().Data.IsCompleted = true;
            LockHandling.Lock(step.Data, new List<LockablePropertyData>());

            // When we subsequently request to lock the property.
            property.RequestLocked(true);

            // Then the property can still be locked.            
            Assert.IsTrue(property.IsLocked);

            yield return null;
        }
    }
}
