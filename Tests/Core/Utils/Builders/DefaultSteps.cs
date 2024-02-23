// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Linq;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Tests.Builder
{
    /// <summary>
    /// Static class to provide fast access to predefined builders.
    /// </summary>
    public static class DefaultSteps
    {
        /// <summary>
        /// Gets the <see cref="ISceneObject"/> with given <paramref name="name"/> from the registry.
        /// </summary>
        /// <param name="name">Name of scene object.</param>
        /// <returns><see cref="ISceneObject"/> with given name.</returns>
        private static ISceneObject GetFromRegistry(string name)
        {
            return RuntimeConfigurator.Configuration.SceneObjectRegistry.GetByTag(Guid.Parse(name)).FirstOrDefault();
        }

        /// <summary>
        /// Get intro step builder.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <returns>Basic step builder with configured name.</returns>
        public static BasicStepBuilder Intro(string name)
        {
            return new BasicStepBuilder(name);
        }
    }
}
