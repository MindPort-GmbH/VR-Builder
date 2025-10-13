// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

namespace VRBuilder.Core.EntityOwners
{
    public interface IEntitySequenceData<TEntity> : IEntityCollectionData<TEntity>, IEntitySequenceData where TEntity : IEntity
    {
        /// <summary>
        /// Current entity in the sequence.
        /// </summary>
        new TEntity Current { get; set; }
    }

    public interface IEntitySequenceData : IData
    {
        /// <summary>
        /// Current entity in the sequence.
        /// </summary>
        IEntity Current { get; }
    }
}
