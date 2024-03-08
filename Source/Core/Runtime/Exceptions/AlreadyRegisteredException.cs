// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Exceptions
{
    [Obsolete("This exception is not used anymore and will be removed in the next major version.")]
    public class AlreadyRegisteredException : ProcessException
    {
        public AlreadyRegisteredException(ISceneObject obj) : base(string.Format("Could not register SceneObject {0}, it's already registered!", obj))
        {
        }
    }
}
