// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Exceptions
{
    [Obsolete("Support for ISceneObject.UniqueName will be removed with VR-Builder 4")]
    public class NameNotUniqueException : ProcessException
    {
        public NameNotUniqueException(ISceneObject entity) : base(string.Format("Could not register Item with name '{0}', name already in use", entity.Guid))
        {
        }
    }
}
