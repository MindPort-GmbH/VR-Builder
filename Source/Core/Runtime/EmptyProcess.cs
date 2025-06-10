// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System.Collections;

namespace VRBuilder.Core
{
    /// <summary>
    /// A stage process that does nothing.
    /// </summary>
    public sealed class EmptyProcess : IStageProcess
    {
        /// <inheritdoc />
        public void Start()
        {
        }

        /// <inheritdoc />
        public IEnumerator Update()
        {
            yield break;
        }

        /// <inheritdoc />
        public void End()
        {
        }

        /// <inheritdoc />
        public void FastForward()
        {
        }
    }
}
