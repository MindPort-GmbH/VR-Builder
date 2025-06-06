﻿// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

namespace VRBuilder.Core.Exceptions
{
    public class InvalidStateException : ProcessException
    {
        public InvalidStateException(string message) : base(message)
        {
        }
    }
}
