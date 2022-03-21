// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections;
using System.Collections.Generic;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Tests.Utils.Mocks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.Utils
{
    public class ChangeModeDuringExecutionTests : RuntimeTests
    {
        private static AudioSource audioSource;

        [SetUp]
        public override void SetUp()
        {
            // Setup the player and its audio source in here.
            // AudioSource.playOnAwake is by default true. Thus audioSource.isPlaying is true during the first frame.
            // The first frame is skipped after setup and audioSource.isPlaying is false as desired.
            GameObject player = new GameObject("AudioPlayer");
            audioSource = player.AddComponent<AudioSource>();

            base.SetUp();
        }

        [UnityTest]
        public IEnumerator ActivationBehavior()
        {
            // Given a linear three step process with 3 ActivationStageBehaviorMock set to Activation and an EndlessConditionMock
            IBehavior behavior1 = new ActivationStageBehaviorMock(BehaviorExecutionStages.Activation);
            IBehavior behavior2 = new ActivationStageBehaviorMock(BehaviorExecutionStages.Activation);
            IBehavior behavior3 = new ActivationStageBehaviorMock(BehaviorExecutionStages.Activation);

            TestLinearChapterBuilder chapterBuilder = TestLinearChapterBuilder.SetupChapterBuilder(3, true);
            chapterBuilder.Steps[0].Data.Behaviors.Data.Behaviors = new List<IBehavior> { behavior1 };
            chapterBuilder.Steps[1].Data.Behaviors.Data.Behaviors = new List<IBehavior> { behavior2 };
            chapterBuilder.Steps[2].Data.Behaviors.Data.Behaviors = new List<IBehavior> { behavior3 };
            IChapter chapter = chapterBuilder.Build();

            IProcess process = new Process("process", chapter);

            // And given a "restricted" and an "unrestricted" mode.
            IMode restricted = new Mode("Restricted", new WhitelistTypeRule<IOptional>().Add<ActivationStageBehaviorMock>());
            IMode unrestricted = new Mode("Unrestricted", new WhitelistTypeRule<IOptional>());

            // When running it and changing the mode during execution several times,
            // Then the corresponding ActivationStageBehaviorMock of the current step is activated and deactivated accordingly.
            // The other ActivationStageBehaviorMock of the other steps stay inactive.
            ProcessRunner.Initialize(process);
            ProcessRunner.Run();
            process.Configure(unrestricted);

            yield return new WaitUntil(() => behavior1.LifeCycle.Stage == Stage.Activating);

            Assert.AreEqual(Stage.Activating, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            process.Configure(restricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            ICondition condition1 = process.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions[0].Data.Conditions[0];
            yield return new WaitUntil(() => condition1.LifeCycle.Stage == Stage.Active);

            condition1.Autocomplete();

            ICondition condition2 = process.Data.FirstChapter.Data.Steps[1].Data.Transitions.Data.Transitions[0].Data.Conditions[0];
            yield return new WaitUntil(() => condition2.LifeCycle.Stage == Stage.Active);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            process.Configure(unrestricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            condition2.Autocomplete();

            ICondition condition3 = process.Data.FirstChapter.Data.Steps[2].Data.Transitions.Data.Transitions[0].Data.Conditions[0];
            yield return new WaitUntil(() => condition3.LifeCycle.Stage == Stage.Active);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior3.LifeCycle.Stage);

            process.Configure(restricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            process.Configure(unrestricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, behavior3.LifeCycle.Stage);

            condition3.Autocomplete();

            yield return new WaitUntil(() => condition3.LifeCycle.Stage == Stage.Inactive);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior3.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator DeactivationBehavior()
        {
            // Given a linear three step process with 3 ActivationStageBehaviorMock set to Activation and an EndlessConditionMock
            IBehavior behavior1 = new ActivationStageBehaviorMock(BehaviorExecutionStages.Deactivation);
            IBehavior behavior2 = new ActivationStageBehaviorMock(BehaviorExecutionStages.Deactivation);
            IBehavior behavior3 = new ActivationStageBehaviorMock(BehaviorExecutionStages.Deactivation);

            TestLinearChapterBuilder chapterBuilder = TestLinearChapterBuilder.SetupChapterBuilder(3, true);
            chapterBuilder.Steps[0].Data.Behaviors.Data.Behaviors = new List<IBehavior> { behavior1 };
            chapterBuilder.Steps[1].Data.Behaviors.Data.Behaviors = new List<IBehavior> { behavior2 };
            chapterBuilder.Steps[2].Data.Behaviors.Data.Behaviors = new List<IBehavior> { behavior3 };
            IChapter chapter = chapterBuilder.Build();

            IProcess process = new Process("process", chapter);

            // And given a "restricted" and an "unrestricted" mode.
            IMode restricted = new Mode("Restricted", new WhitelistTypeRule<IOptional>().Add<ActivationStageBehaviorMock>());
            IMode unrestricted = new Mode("Unrestricted", new WhitelistTypeRule<IOptional>());

            // When running it and changing the mode during execution several times,
            // Then the corresponding ActivationStageBehaviorMock of the current step is activated and deactivated accordingly.
            // The other ActivationStageBehaviorMock of the other steps stay inactive.
            ProcessRunner.Initialize(process);
            ProcessRunner.Run();
            process.Configure(unrestricted);

            ICondition condition1 = process.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions[0].Data.Conditions[0];
            yield return new WaitUntil(() => condition1.LifeCycle.Stage == Stage.Active);

            Assert.AreEqual(Stage.Active, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            process.Configure(restricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            condition1.Autocomplete();
            yield return null;

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            ICondition condition2 = process.Data.FirstChapter.Data.Steps[1].Data.Transitions.Data.Transitions[0].Data.Conditions[0];
            yield return new WaitUntil(() => condition2.LifeCycle.Stage == Stage.Active);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            process.Configure(unrestricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            condition2.Autocomplete();
            yield return null;

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            ICondition condition3 = process.Data.FirstChapter.Data.Steps[2].Data.Transitions.Data.Transitions[0].Data.Conditions[0];
            yield return new WaitUntil(() => condition3.LifeCycle.Stage == Stage.Active);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior3.LifeCycle.Stage);

            process.Configure(restricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);

            process.Configure(unrestricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, behavior3.LifeCycle.Stage);

            condition3.Autocomplete();
            yield return null;

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior3.LifeCycle.Stage);

            process.Configure(restricted);

            Assert.AreEqual(Stage.Inactive, behavior1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Inactive, behavior3.LifeCycle.Stage);
        }
    }
}
