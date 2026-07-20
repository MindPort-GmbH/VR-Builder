// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using Source.Core.Runtime.Helpers;
using UnityEngine;
using VRBuilder.Core.Helpers;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Settings;

namespace Source.Core.Runtime.Configuration
{
    [CreateAssetMenu(fileName = "SceneObjectRegistry", menuName = "VR Builder/Scene Object Registry", order = 2)]
    public class SceneObjectRegistrySettings : SettingsObject<SceneObjectRegistrySettings>, ISceneObjectRegistryConfiguration
    {
        [SerializeField]
        private string serviceTypeName = typeof(SceneObjectRegistry).FullName;

        public string ServiceTypeName => serviceTypeName;

        public ISceneObjectFinder SceneObjectFinder { get; } = new SceneObjectFinder();
        public ISceneObjectIdentity SceneObjectIdentity { get; } = new SceneObjectIdentity();
        public IEditorPrefabHandler EditorPrefabHandler { get; } = new EditorPrefabHandler();
    }
}