// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Helpers;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace Source.Core.Runtime.Helpers
{
    /// <inheritdoc/>
    public class SceneObjectFinder : ISceneObjectFinder
    {
        /// <inheritdoc/>
        public IEnumerable<T> FindAllSceneObjects<T>() where T : class
        {
            return SceneUtils.GetActiveAndInactiveComponents<ProcessSceneObject>().OfType<T>();
        }
    }
}