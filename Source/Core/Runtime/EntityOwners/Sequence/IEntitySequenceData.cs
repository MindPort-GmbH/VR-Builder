// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

namespace VRBuilder.Core.EntityOwners
{
    public interface IEntitySequenceData<TEntity> : IEntityCollectionData<TEntity> where TEntity : IEntity
    {
        TEntity Current { get; set; }
    }
}
