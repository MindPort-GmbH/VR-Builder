// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Runtime.Serialization;
using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Tests.Utils.Mocks
{
    /// <summary>
    /// Helper behavior for testing that has a reference object.
    /// </summary>
    public class ObjectReferenceBehaviorMock : Behavior<ObjectReferenceBehaviorMock.EntityData>
    {
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Target scene object to be referenced.
            /// </summary>
            [Obsolete("Use SingleReferenceObject instead.")]
            public ScenePropertyReference<ISceneObjectProperty> ReferenceObject { get; set; }

            [DataMember]
            public SingleScenePropertyReference<ISceneObjectProperty> SingleReferenceObject { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        public ObjectReferenceBehaviorMock(Guid sceneObjectId)
        {
            Data.SingleReferenceObject = new SingleScenePropertyReference<ISceneObjectProperty>(sceneObjectId);
        }
    }
}