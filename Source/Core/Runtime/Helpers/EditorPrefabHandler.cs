// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;
using VRBuilder.Core.Helpers;
using VRBuilder.Core.SceneObjects;

namespace Source.Core.Runtime.Helpers
{
    /// <inheritdoc/>
    public class EditorPrefabHandler : IEditorPrefabHandler
    {
        /// <inheritdoc/>
        public void OnDuplicateGuidDetected(ISceneObject sceneObject)
        {
            var go = (sceneObject as ProcessSceneObject)?.GameObject;
            EditorUtility.SetDirty(go);
            if (PrefabUtility.IsPartOfPrefabInstance(go))
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    PrefabUtility.GetOutermostPrefabInstanceRoot(go));
        }
    }
}