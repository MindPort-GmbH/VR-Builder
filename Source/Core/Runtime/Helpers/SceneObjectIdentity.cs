// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using VRBuilder.Core.Helpers;
using VRBuilder.Core.SceneObjects;

namespace Source.Core.Runtime.Helpers
{
    /// <inheritdoc/>
    public class SceneObjectIdentity : ISceneObjectIdentity
    {
        /// <inheritdoc/>
        public ulong GetIdentity(ISceneObject obj)
        {
            if (obj is ProcessSceneObject sceneObject)
                return (ulong)sceneObject.GetInstanceID();
            return 0;
        }
    }
}