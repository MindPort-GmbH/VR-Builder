﻿// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System.Runtime.Serialization;

namespace VRBuilder.Core.Behaviors
{
    [DataContract(IsReference = true)]
    public class BehaviorMetadata : IMetadata
    {
        [DataMember]
        public bool IsFoldedOut { get; set; }
    }
}
