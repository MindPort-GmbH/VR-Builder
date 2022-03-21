// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core;

namespace VRBuilder.Tests.Builder
{
    /// <summary>
    /// Builder for a linear chapter.
    /// </summary>
    public class LinearChapterBuilder : ChapterBuilder<Chapter>
    {
        protected IStep FirstStep { get; set; }
        protected IStep LastStep { get; set; }
        protected List<IStep> Steps { get; set; }

        /// <summary>
        /// This builder will create a chapter with given name.
        /// </summary>
        /// <param name="name">Name of a chapter.</param>
        public LinearChapterBuilder(string name) : base(name)
        {
            Steps = new List<IStep>();

            AddSecondPassAction(() =>
            {
                Result = new Chapter(name, FirstStep);
                Result.Data.Steps = Steps.ToList();
            });
        }

        /// <summary>
        /// Adds a step to a chapter.
        /// </summary>
        /// <typeparam name="TStep">Type of step to add to chapter.</typeparam>
        /// <param name="stepBuilder">Builder for a step.</param>
        /// <returns>Itself as implementation of method chaining pattern.</returns>
        public LinearChapterBuilder AddStep<TStep>(StepBuilder<TStep> stepBuilder) where TStep : IStep
        {
            AddFirstPassAction(() =>
            {
                stepBuilder.SetRelativeResourcePathAction(() => ResourcePath);
            });

            AddFirstPassAction(() =>
            {
                TStep step = stepBuilder.Build();

                if (FirstStep == null)
                {
                    FirstStep = step;
                }

                if (LastStep != null)
                {
                    LastStep.Data.Transitions.Data.Transitions.First().Data.TargetStep = step;
                }

                LastStep = step;

                Steps.Add(step);
            });

            return this;
        }

        public new LinearChapterBuilder SetResourcePath(string path)
        {
            base.SetResourcePath(path);
            return this;
        }

        /// <inheritdoc />
        protected override void Cleanup()
        {
            base.Cleanup();
            FirstStep = null;
            LastStep = null;
        }
    }
}
