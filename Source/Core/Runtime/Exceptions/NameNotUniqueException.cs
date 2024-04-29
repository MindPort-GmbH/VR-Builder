// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Exceptions
{
    [Obsolete("This exception is no longer used and will be removed in the next major version.")]
    public class NameNotUniqueException : ProcessException
    {
        public NameNotUniqueException(ISceneObject entity) : base(string.Format("Could not register Item with name '{0}', name already in use", entity.UniqueName))
        {
        }
    }
}
