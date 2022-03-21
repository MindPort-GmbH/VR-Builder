// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections;
using System.Collections.Generic;
using VRBuilder.Core;
using VRBuilder.Core.Configuration;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using VRBuilder.Tests.Builder;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.Processes
{
    public class BaseProcessTests : RuntimeTests
    {
        [Test]
        public void CanBeSetup()
        {
            Chapter chapter1 = TestLinearChapterBuilder.SetupChapterBuilder(3, false).Build();
            Chapter chapter2 = TestLinearChapterBuilder.SetupChapterBuilder().Build();
            Process process = new Process("MyProcess", new List<IChapter>
            {
                chapter1,
                chapter2
            });

            Assert.AreEqual(chapter1, process.Data.FirstChapter);
        }

        [UnityTest]
        public IEnumerator OneChapterProcess()
        {
            Chapter chapter1 = TestLinearChapterBuilder.SetupChapterBuilder(3, false).Build();
            Process process = new Process("MyProcess", chapter1);

            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            Debug.Log(chapter1.LifeCycle.Stage);
            yield return null;

            Assert.AreEqual(Stage.Activating, chapter1.LifeCycle.Stage);

            while (chapter1.LifeCycle.Stage != Stage.Inactive)
            {
                Debug.Log(chapter1.LifeCycle.Stage);
                yield return null;
            }

            Assert.AreEqual(Stage.Inactive, chapter1.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator TwoChapterProcess()
        {
            Chapter chapter1 = TestLinearChapterBuilder.SetupChapterBuilder(3, false).Build();
            Chapter chapter2 = TestLinearChapterBuilder.SetupChapterBuilder().Build();
            Process process = new Process("MyProcess", new List<IChapter>
            {
                chapter1,
                chapter2
            });

            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            yield return new WaitUntil(() => chapter1.LifeCycle.Stage == Stage.Activating);

            Assert.AreEqual(Stage.Inactive, chapter2.LifeCycle.Stage);

            yield return new WaitUntil(() => chapter2.LifeCycle.Stage == Stage.Activating);

            Assert.AreEqual(Stage.Inactive, chapter1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, chapter2.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator EventsAreThrown()
        {
            Chapter chapter1 = TestLinearChapterBuilder.SetupChapterBuilder(3, false).Build();
            Chapter chapter2 = TestLinearChapterBuilder.SetupChapterBuilder(3, false).Build();
            Process process = new Process("MyProcess", new List<IChapter>
            {
                chapter1,
                chapter2
            });

            bool wasStarted = false;
            bool wasCompleted = false;

            process.LifeCycle.StageChanged += (obj, args) =>
            {
                if (args.Stage == Stage.Activating)
                {
                    wasStarted = true;
                }
                else if (args.Stage == Stage.Active)
                {
                    wasCompleted = true;
                }
            };

            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            while (process.LifeCycle.Stage != Stage.Inactive)
            {
                yield return null;
            }

            Assert.IsTrue(wasStarted);
            Assert.IsTrue(wasCompleted);
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveProcess()
        {
            // Given a process
            Process process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new EndlessConditionMock())))
                .Build();

            process.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When you mark it to fast-forward,
            process.LifeCycle.MarkToFastForward();

            // Then it doesn't autocomplete because it weren't activated yet.
            Assert.AreEqual(Stage.Inactive, process.LifeCycle.Stage);
            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveProcessAndActivateIt()
        {
            // Given a process
            Process process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new EndlessConditionMock())))
                .Build();

            // When you mark it to fast-forward and activate it,
            process.LifeCycle.MarkToFastForward();

            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            yield return null;

            // Then it autocompletes.
            Assert.AreEqual(Stage.Inactive, process.LifeCycle.Stage);
            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingProcess()
        {
            // Given an activated process
            Process process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new EndlessConditionMock())))
                .Build();

            ProcessRunner.Initialize(process);
            ProcessRunner.Run();

            // When you mark it to fast-forward,
            process.LifeCycle.MarkToFastForward();

            // Then it finishes activation.
            Assert.AreEqual(Stage.Active, process.LifeCycle.Stage);
            yield break;
        }
    }
}
