// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

namespace VRBuilder.Core
{
    /// <summary>
    /// Interface of a process.
    /// </summary>
    public interface IProcess : IEntity, IDataOwner<IProcessData>, IClonable<IProcess>
    {
        /// <summary>
        /// Utility data used by editor.
        /// </summary>
        ProcessMetadata ProcessMetadata { get; }
    }
}
