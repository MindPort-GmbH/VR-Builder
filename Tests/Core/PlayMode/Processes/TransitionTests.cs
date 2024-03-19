// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System.Collections;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.Processes
{
    public class TransitionTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator SkipSingleConditionBeforeActivation()
        {
            // Given an inactive transition with a skipped condition,
            Transition transition = new Transition();
            transition.Data.Conditions.Add(new OptionalEndlessConditionMock());

            transition.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessConditionMock>()));

            // When the transition is activated,
            transition.LifeCycle.Activate();

            while (transition.IsCompleted == false)
            {
                yield return null;
                transition.Update();
            }

            // Then it is immediately completed.
            Assert.IsTrue(transition.IsCompleted);

            yield break;
        }

        [UnityTest]
        public IEnumerator SkipSingleConditionDuringActivation()
        {
            // Given an activating transition with a condition,
            Transition transition = new Transition();
            transition.Data.Conditions.Add(new OptionalEndlessConditionMock());
            transition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            transition.LifeCycle.Activate();

            while (transition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                transition.Update();
            }

            // When the condition is skipped,
            transition.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessConditionMock>()));

            yield return null;
            transition.Update();

            // Then the transition immediately completes.
            Assert.IsTrue(transition.IsCompleted);

            yield break;
        }

        [UnityTest]
        public IEnumerator SkipOneOfTheConditionsDuringActivation()
        {
            // Given an activating transition with a condition,
            Transition transition = new Transition();
            transition.Data.Conditions.Add(new OptionalEndlessConditionMock());
            transition.Data.Conditions.Add(new EndlessConditionMock());
            transition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            transition.LifeCycle.Activate();

            while (transition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                transition.Update();
            }

            // When the condition is skipped,
            transition.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessConditionMock>()));

            yield return null;
            transition.Update();

            // Then the transition is not completed, as the second condition was never completed.
            Assert.IsFalse(transition.IsCompleted);

            yield break;
        }

        [UnityTest]
        public IEnumerator InactiveConditionDoesntPreventCompletion()
        {
            EndlessConditionMock notOptional = new EndlessConditionMock();

            // Given an activating transition with a condition,
            Transition transition = new Transition();
            transition.Data.Conditions.Add(new OptionalEndlessConditionMock());
            transition.Data.Conditions.Add(notOptional);
            transition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            transition.LifeCycle.Activate();

            while (transition.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                transition.Update();
            }

            // When the condition is skipped and the second condition is completed,
            transition.Configure(new Mode("Test", new WhitelistTypeRule<IOptional>().Add<OptionalEndlessConditionMock>()));

            notOptional.Autocomplete();

            yield return null;
            transition.Update();

            // Then the transition is completed.
            Assert.IsTrue(transition.IsCompleted);

            yield break;
        }

        [UnityTest]
        public IEnumerator MultiConditionTransitionFinishes()
        {
            // Given a transition with two conditions,
            EndlessConditionMock condition1 = new EndlessConditionMock();
            EndlessConditionMock condition2 = new EndlessConditionMock();
            Transition transition = new Transition();
            transition.Data.Conditions.Add(condition1);
            transition.Data.Conditions.Add(condition2);
            transition.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // After it is activated and the conditions are completed,
            transition.LifeCycle.Activate();

            yield return null;
            transition.Update();

            condition1.Autocomplete();

            Assert.IsTrue(condition1.IsCompleted);
            Assert.IsFalse(condition2.IsCompleted);
            Assert.IsFalse(transition.IsCompleted);

            condition2.Autocomplete();

            Assert.IsTrue(condition1.IsCompleted);
            Assert.IsTrue(condition2.IsCompleted);
            Assert.IsFalse(transition.IsCompleted);

            while (transition.IsCompleted == false)
            {
                yield return null;
                transition.Update();
            }

            // Then and only then the transition is completed.
            Assert.IsTrue(transition.IsCompleted);
        }
    }
}
