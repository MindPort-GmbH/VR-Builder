// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections;
using System.Linq;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Tests.Builder;
using VRBuilder.Tests.Utils;
using VRBuilder.Tests.Utils.Mocks;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace VRBuilder.Tests.Serialization
{
    public class JsonProcessSerializerTests : RuntimeTests
    {
        [UnityTest]
        public IEnumerator BaseProcess()
        {
            // Given base process
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")))
                .Build();

            Serializer.ProcessToByteArray(process1);

            // When we serialize and deserialize it
            IProcess process2 = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process1));

            // Then it should still be base process, have the same name and the first chapter with the same name.
            Assert.AreEqual(typeof(Process), process1.GetType());
            Assert.AreEqual(process1.GetType(), process2.GetType());

            Assert.AreEqual(process1.Data.Name, "Process");
            Assert.AreEqual(process1.Data.Name, process2.Data.Name);

            Assert.AreEqual(process1.Data.FirstChapter.Data.Name, "Chapter");
            Assert.AreEqual(process1.Data.FirstChapter.Data.Name, process2.Data.FirstChapter.Data.Name);

            return null;
        }

        [UnityTest]
        public IEnumerator Chapter()
        {
            // Given we have a process with a chapter
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")))
                .Build();

            // When we serialize and deserialize it
            IProcess process2 = Serializer.ProcessFromByteArray((Serializer.ProcessToByteArray(process1)));

            // Then chapter's type, name, first step and next chapter should not change.
            IChapter chapter1 = process1.Data.FirstChapter;
            IChapter chapter2 = process2.Data.FirstChapter;

            Assert.AreEqual(chapter1.GetType(), chapter2.GetType());
            Assert.AreEqual(chapter1.Data.Name, chapter2.Data.Name);
            Assert.AreEqual(chapter1.Data.FirstStep.Data.Name, chapter2.Data.FirstStep.Data.Name);
            Assert.AreEqual(process1.Data.Chapters.Count, process2.Data.Chapters.Count);

            return null;
        }

        [UnityTest]
        public IEnumerator Condition()
        {
            // Given a process which has a step with a condition
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new AutoCompletedCondition())))
                .Build();

            // When we serialize and deserialize it
            IProcess process2 = Serializer.ProcessFromByteArray((Serializer.ProcessToByteArray(process1)));

            // Then that condition's name should not change.
            ICondition condition1 = process1.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First()
                .Data.Conditions.First();
            ICondition condition2 = process2.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First()
                .Data.Conditions.First();

            Assert.AreEqual(condition1.GetType(), condition2.GetType());

            return null;
        }

        [UnityTest]
        public IEnumerator Transition()
        {
            // Given a process with more than one step
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("FirstStep"))
                    .AddStep(new BasicStepBuilder("SecondStep")))
                .Build();

            // When we serialize and deserialize it
            byte[] serialized = Serializer.ProcessToByteArray(process1);
            IProcess process2 = Serializer.ProcessFromByteArray(serialized);

            // Then transition from the first step should lead to the same step as before.
            Assert.AreEqual(
                process1.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.TargetStep
                    .Data.Name,
                process2.Data.FirstChapter.Data.FirstStep.Data.Transitions.Data.Transitions.First().Data.TargetStep
                    .Data.Name);

            return null;
        }

        [UnityTest]
        public IEnumerator Step()
        {
            // Given we have a process with a step
            IProcess process1 = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddCondition(new AutoCompletedCondition())))
                .Build();

            // When we serialize and deserialize it
            IProcess process2 = Serializer.ProcessFromByteArray((Serializer.ProcessToByteArray(process1)));

            // Then that step's name should still be the same.
            Assert.AreEqual(process1.Data.FirstChapter.Data.FirstStep.Data.Name,
                process2.Data.FirstChapter.Data.FirstStep.Data.Name);

            return null;
        }
    }
}
