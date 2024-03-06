// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;

namespace VRBuilder.Tests.Builder
{
    /// <summary>
    /// Abstract Builder with ResourcePath property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BuilderWithResourcePath<T> : Builder<T>
    {
        public string ResourcePath { get; protected set; }
        protected string ResourceSubPath { get; set; }

        public BuilderWithResourcePath(string resourceSubPath)
        {
            ResourceSubPath = resourceSubPath;
        }

        /// <summary>
        /// Combine ResourcePath using the result of getPath() call and ResourceSubPath. Used mostly for internal purposes.
        /// </summary>
        /// <param name="getPath">Delegate which returns parent directory.</param>
        public virtual void SetRelativeResourcePathAction(Func<string> getPath)
        {
            string path = getPath();

            if (path == null)
            {
                return;
            }

            ResourcePath = path;

            if (path.Length > 0 && path[path.Length - 1] != '/')
            {
                ResourcePath += '/';
            }

            ResourcePath += ResourceSubPath;
        }

        /// <summary>
        /// Queues an Action to set ResourcePath.
        /// </summary>
        /// <param name="path">value to set ResourcePath to.</param>
        /// <returns>this.</returns>
        public BuilderWithResourcePath<T> SetResourcePath(string path)
        {
            AddFirstPassAction(() => ResourcePath = path);
            return this;
        }
    }
}
