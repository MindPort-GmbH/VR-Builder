// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Linq;
using VRBuilder.Core;
using VRBuilder.Core.Serialization;
using VRBuilder.Tests.Builder;
using NUnit.Framework;

namespace VRBuilder.Editor.Tests
{
    public class SerializationLengthTests
    {

        [Test]
        public void RunSerializerWithLinear()
        {
            int length = 100;
            IProcess process = CreateProcess(length);

            ImprovedNewtonsoftJsonProcessSerializer serializer = new ImprovedNewtonsoftJsonProcessSerializer();
            try
            {
                byte[] data = serializer.ProcessToByteArray(process);
                serializer.ProcessFromByteArray(data);
            }
            catch (Exception)
            {
                Assert.Fail("failed with a length of: " + length);
            }
        }

        [Test]
        public void RunSerializerWithSplit()
        {
            int length = 200;
            IProcess process = CreateSplitProcess(length);

            ImprovedNewtonsoftJsonProcessSerializer serializer = new ImprovedNewtonsoftJsonProcessSerializer();
            try
            {
                byte[] data = serializer.ProcessToByteArray(process);
                serializer.ProcessFromByteArray(data);
            }
            catch (Exception)
            {
                Assert.Fail("failed with a length of: " + length);
            }
        }

        [Test]
        public void RunSerializerWithEarlyFinish()
        {
            int length = 500;
            IProcess process = CreateProcess(length);

            ImprovedNewtonsoftJsonProcessSerializer serializer = new ImprovedNewtonsoftJsonProcessSerializer();
            try
            {
                Transition t1 = new Transition();
                t1.Data.TargetStep = null;
                process.Data.Chapters[0].Data.Steps.First().Data.Transitions.Data.Transitions.Insert(0, t1);

                byte[] data = serializer.ProcessToByteArray(process);
                serializer.ProcessFromByteArray(data);
            }
            catch (Exception)
            {
                Assert.Fail("failed with a length of: " + length);
            }
        }

        private IProcess CreateProcess(int length)
        {

            LinearChapterBuilder chapterBuilder = new LinearChapterBuilder("chapter");
            for (int i = 0; i < length; i++)
            {
                chapterBuilder.AddStep(new BasicStepBuilder("Step#" + i));
            }

            return new LinearProcessBuilder("Process")
                .AddChapter(chapterBuilder)
                .Build();
        }

        private IProcess CreateSplitProcess(int length)
        {

            LinearChapterBuilder[] chapterBuilder = new[] {new LinearChapterBuilder("chapter"), new LinearChapterBuilder("chapter"), new LinearChapterBuilder("chapter")};

            for (int c = 0; c < 3; c++)
            {
                for (int i = 0; i < length; i++)
                {
                    chapterBuilder[c].AddStep(new BasicStepBuilder("Step#" + i));
                }
            }

            Chapter chapter = chapterBuilder[0].Build();


            Chapter c1 = chapterBuilder[1].Build();
            Transition t1 = new Transition();
            t1.Data.TargetStep = c1.Data.FirstStep;


            Chapter c2 = chapterBuilder[1].Build();
            Transition t2 = new Transition();
            t2.Data.TargetStep = c2.Data.FirstStep;


            chapter.Data.FirstStep.Data.Transitions.Data.Transitions.Add(t1);
            chapter.Data.FirstStep.Data.Transitions.Data.Transitions.Add(t2);


            Transition t1end = new Transition();
            t1end.Data.TargetStep = chapter.Data.Steps.Last();
            c1.Data.Steps.Last().Data.Transitions.Data.Transitions.Add(t1end);


            Transition t2end = new Transition();
            t2end.Data.TargetStep = chapter.Data.Steps.Last();
            c2.Data.Steps.Last().Data.Transitions.Data.Transitions.Add(t2end);

            c1.Data.Steps.ToList().ForEach(chapter.Data.Steps.Add);
            c2.Data.Steps.ToList().ForEach(chapter.Data.Steps.Add);

            return new Process("name", chapter);
        }
    }
}
