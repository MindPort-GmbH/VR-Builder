// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Utils
{
    public static class ProcessReferenceUtils
    {
        [Obsolete]
        public static string GetNameFrom(ISceneObjectProperty property)
        {
            if (property == null)
            {
                return null;
            }

            if (property.SceneObject == null)
            {
                return null;
            }

            return property.SceneObject.UniqueName;
        }

        public static Guid GetUniqueIdFrom(ISceneObjectProperty property)
        {
            if (property == null)
            {
                return Guid.Empty;
            }

            return GetUniqueIdFrom(property.SceneObject);
        }

        [Obsolete]
        public static string GetNameFrom(ISceneObject sceneObject)
        {
            if (sceneObject == null)
            {
                return null;
            }

            return sceneObject.UniqueName;
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
