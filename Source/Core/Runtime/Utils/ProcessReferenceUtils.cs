// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Utils
{
    public static class ProcessReferenceUtils
    {
        public static Guid GetUniqueIdFrom(ISceneObjectProperty property)
        {
            if (property == null)
            {
                return Guid.Empty;
            }

            return GetUniqueIdFrom(property.SceneObject);
        }

        public static Guid GetUniqueIdFrom(ISceneObject sceneObject)
        {
            if (sceneObject == null)
            {
                return Guid.Empty;
            }

            return sceneObject.Guid;
        }
    }
}
