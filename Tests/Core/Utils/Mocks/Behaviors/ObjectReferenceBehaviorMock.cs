// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using VRBuilder.Core;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Tests.Utils.Mocks
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
            public ScenePropertyReference<ISceneObjectProperty> ReferenceObject { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        public ObjectReferenceBehaviorMock(string sceneObjectName)
        {
            Data.ReferenceObject = new ScenePropertyReference<ISceneObjectProperty>(sceneObjectName);
        }
    }
}
