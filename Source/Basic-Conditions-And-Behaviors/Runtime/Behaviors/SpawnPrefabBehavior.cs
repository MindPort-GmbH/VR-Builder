using Newtonsoft.Json;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;

namespace VRBuilder.Animations.Behaviors
{
    /// <summary>
    /// Spawns a prefab in a Resources folder, given its relative path.
    /// </summary>
    [DataContract(IsReference = true)]
    [HelpLink("https://www.mindport.co/vr-builder-tutorials/animations-add-on")]
    public class SpawnPrefabBehavior : Behavior<SpawnPrefabBehavior.EntityData>
    {
        /// <summary>
        /// The <see cref="SpawnPrefabBehavior"/> behavior's data.
        /// </summary>
        [DisplayName("Spawn Prefab")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Path of the prefab to spawn.
            /// </summary>
            [DataMember]
            [DisplayName("Prefab resource path")]
            public string Path { get; set; }

            /// <summary>
            /// Reference to position provider.
            /// </summary>
            [DataMember]
            [DisplayName("Position provider")]
            public SceneObjectReference Position { get; set; }

            /// <inheritdoc />
            public Metadata Metadata { get; set; }

            /// <inheritdoc />
            public string Name { get; set; }
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            GameObject prefab;

            public ActivatingProcess(EntityData data) : base(data)
            {
                prefab = Resources.Load<GameObject>(data.Path);
            }

            /// <inheritdoc />
            public override void Start()
            {
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
                GameObject instantiatedPrefab = GameObject.Instantiate<GameObject>(prefab);
                instantiatedPrefab.transform.position = Data.Position.Value.GameObject.transform.position;
                instantiatedPrefab.transform.rotation = Data.Position.Value.GameObject.transform.rotation;                
            }

            public override void FastForward()
            {
            }
        }

        [JsonConstructor, Preserve]
        public SpawnPrefabBehavior() : this("", "")
        {            
        }

        public SpawnPrefabBehavior(string prefabPath, ISceneObject positionProvider, string name = "Spawn Prefab") : this (prefabPath, ProcessReferenceUtils.GetNameFrom(positionProvider), name)
        {
        }

        public SpawnPrefabBehavior(string prefabPath, string positionProviderName, string name = "Spawn Prefab")
        {
            Data.Path = prefabPath;
            Data.Position = new SceneObjectReference(positionProviderName);
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
