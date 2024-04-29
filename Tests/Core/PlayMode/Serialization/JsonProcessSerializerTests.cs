// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH
using System.Collections;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.Tests.RuntimeUtils;
using VRBuilder.Core.Tests.Utils.Builders;
using VRBuilder.Core.Tests.Utils.Mocks;

namespace VRBuilder.Core.Tests.Serialization
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

            // Then chapter's type, name, first step, guid and next chapter should not change.
            IChapter chapter1 = process1.Data.FirstChapter;
            IChapter chapter2 = process2.Data.FirstChapter;

            Assert.AreEqual(chapter1.GetType(), chapter2.GetType());
            Assert.AreEqual(chapter1.Data.Name, chapter2.Data.Name);
            Assert.AreEqual(chapter1.Data.FirstStep.Data.Name, chapter2.Data.FirstStep.Data.Name);
            Assert.AreEqual(process1.Data.Chapters.Count, process2.Data.Chapters.Count);
            Assert.AreEqual(chapter1.ChapterMetadata.Guid, chapter2.ChapterMetadata.Guid);

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

            // Then that step's name and guid should still be the same.
            Assert.AreEqual(process1.Data.FirstChapter.Data.FirstStep.Data.Name,
                process2.Data.FirstChapter.Data.FirstStep.Data.Name);
            Assert.AreEqual(process1.Data.FirstChapter.Data.FirstStep.StepMetadata.Guid,
                process2.Data.FirstChapter.Data.FirstStep.StepMetadata.Guid);

            return null;
        }

        [UnityTest]
        public IEnumerator NestedChapter()
        {
            // Given a chapter nested in a step
            IChapter nestedChapter = new LinearChapterBuilder("NestedChapter")
                .AddStep(new BasicStepBuilder("Step 1"))
                .Build();

            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new ExecuteChaptersBehavior(new[] { nestedChapter }))))
                .Build();

            // When we serialize and deserialize a process with it
            IProcess testProcess = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process));

            // Then chapter's type, name, first step and next chapter should not change.
            EntityCollectionData<IChapter> data = testProcess.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First(behavior => behavior is ExecuteChaptersBehavior).Data as EntityCollectionData<IChapter>;
            Assert.IsNotNull(data);

            IChapter nested = data.GetChildren().FirstOrDefault();
            Assert.IsNotNull(nested);
            Assert.AreEqual(nested.ChapterMetadata.Guid, nestedChapter.ChapterMetadata.Guid);
            Assert.AreEqual(nested.GetType(), nestedChapter.GetType());
            Assert.AreEqual(nested.Data.FirstStep.Data.Name, nestedChapter.Data.FirstStep.Data.Name);
            Assert.AreEqual(nested.Data.FirstStep.StepMetadata.Guid, nestedChapter.Data.FirstStep.StepMetadata.Guid);

            return null;
        }

        [UnityTest]
        public IEnumerator NestedNestedChapter()
        {
            // Given a chapter nested in a step which is itself in a subchapter
            IChapter level2 = new LinearChapterBuilder("Level 2")
                .AddStep(new BasicStepBuilder("L2Step"))
                .Build();

            IChapter level1 = new LinearChapterBuilder("Level 1")
                .AddStep(new BasicStepBuilder("L1Step")
                    .AddBehavior(new ExecuteChaptersBehavior(new[] { level2 })))
                .Build();

            IProcess process = new LinearProcessBuilder("Process")
                .AddChapter(new LinearChapterBuilder("Chapter")
                    .AddStep(new BasicStepBuilder("Step")
                        .AddBehavior(new ExecuteChaptersBehavior(new[] { level1 }))))
                .Build();

            // When we serialize and deserialize a process with it
            IProcess testProcess = Serializer.ProcessFromByteArray(Serializer.ProcessToByteArray(process));

            // Then chapter's type, name, first step and next chapter should not change.
            EntityCollectionData<IChapter> data1 = testProcess.Data.FirstChapter.Data.FirstStep.Data.Behaviors.Data.Behaviors.First(behavior => behavior is ExecuteChaptersBehavior).Data as EntityCollectionData<IChapter>;
            Assert.IsNotNull(data1);

            IChapter l1 = data1.GetChildren().FirstOrDefault();
            EntityCollectionData<IChapter> data2 = l1.Data.FirstStep.Data.Behaviors.Data.Behaviors.First(behavior => behavior is ExecuteChaptersBehavior).Data as EntityCollectionData<IChapter>;
            Assert.IsNotNull(data2);

            IChapter l2 = data2.GetChildren().FirstOrDefault();
            Assert.IsNotNull(l2);
            Assert.AreEqual(l2.ChapterMetadata.Guid, level2.ChapterMetadata.Guid);
            Assert.AreEqual(l2.GetType(), level2.GetType());
            Assert.AreEqual(l2.Data.FirstStep.Data.Name, level2.Data.FirstStep.Data.Name);
            Assert.AreEqual(l2.Data.FirstStep.StepMetadata.Guid, level2.Data.FirstStep.StepMetadata.Guid);

            return null;
        }
    }
}
