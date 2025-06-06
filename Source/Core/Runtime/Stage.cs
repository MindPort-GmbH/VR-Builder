// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

namespace VRBuilder.Core
{
    /// <summary>
    /// All possible states of an <see cref="IEntity"/>.
    /// </summary>
    public enum Stage
    {
        Inactive,
        Activating,
        Active,
        Deactivating,
        Aborting
    }
}
