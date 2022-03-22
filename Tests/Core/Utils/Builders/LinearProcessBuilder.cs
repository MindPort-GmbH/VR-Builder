// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections.Generic;
using VRBuilder.Core;

namespace VRBuilder.Tests.Builder
{
    /// <summary>
    /// Builder that creates linear processes.
    /// </summary>
    public class LinearProcessBuilder : ProcessBuilder<Process>
    {
        public List<IChapter> Chapters { get; set; }

        /// <summary>
        /// The builder will create a process with given name.
        /// </summary>
        /// <param name="name">Name of the process.</param>
        public LinearProcessBuilder(string name, string rootResourceFolder = "") : base(name)
        {
            Chapters = new List<IChapter>();
            AddFirstPassAction(() => SetRelativeResourcePathAction(() => rootResourceFolder));
            AddSecondPassAction(() => Result = new Process(name, Chapters));
        }

        /// <summary>
        /// Add an Action to an execution queue that makes necessary operations over a chapter it gets from the chapterBuilder.
        /// </summary>
        public LinearProcessBuilder AddChapter<TChapter>(ChapterBuilder<TChapter> chapterBuilder) where TChapter : IChapter
        {
            AddFirstPassAction(() =>
            {
                chapterBuilder.SetRelativeResourcePathAction(() => ResourcePath);
            });

            AddFirstPassAction(() =>
            {
                Chapters.Add(chapterBuilder.Build());
            });
            return this;
        }

        public new LinearProcessBuilder SetResourcePath(string path)
        {
            base.SetResourcePath(path);
            return this;
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            base.Cleanup();
            Chapters.Clear();
        }

    }
}
