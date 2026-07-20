// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Editor.Source.Core.Editor
{
    /// <summary>
    /// We need something during Editor Time that clears the Dirty State that we might in after we modified a SceneObject in Prefab.
    /// This Class Provides it.
    /// </summary>
    [InitializeOnLoad]
    public class EditorRegistryHelper
    {
        static EditorRegistryHelper()
        {
            EditorApplication.update += () => (SceneObjectRegistryLocator.Current as SceneObjectRegistry)?.RefreshIfDirty();
        }
    }
}