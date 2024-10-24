using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// Behavior that loads the specified scene, either additively or not.
    /// Loading a scene not additively interrupts the current process.
    /// </summary>
    [DataContract(IsReference = true)]
    public class LoadSceneBehavior : Behavior<LoadSceneBehavior.EntityData>
    {
        /// <summary>
        /// The data class for a load scene behavior.
        /// </summary>        
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            /// <summary>
            /// Asset path of the scene to load.
            /// </summary>
            [DataMember]
            [UsesSpecificProcessDrawer("SceneDropdownDrawer")]
            [DisplayName("Scene to load")]
            public string ScenePath { get; set; }

            /// <summary>
            /// If true, the scene will be loaded additively.
            /// </summary>
            [DataMember]
            [DisplayName("Load additively")]
            public bool LoadAdditively { get; set; }

            /// <summary>
            /// If true, the scene will be loaded asynchronously during the update cycle of the stage process.
            /// </summary>
            [DataMember]
            [DisplayName("Load asynchronously")]
            public bool LoadAsynchronously { get; set; }

            public Metadata Metadata { get; set; }

            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string sceneName = string.IsNullOrEmpty(ScenePath) ? "[NULL]" : Path.GetFileNameWithoutExtension(ScenePath);
                    string additively = LoadAdditively ? " additively" : "";

                    return $"Load scene '{sceneName}'{additively}";
                }
            }
        }

        [JsonConstructor, Preserve]
        public LoadSceneBehavior()
        {
        }

        private class ActivatingProcess : StageProcess<EntityData>
        {
            bool isLoading = false;
            LoadSceneMode loadSceneMode;

            public ActivatingProcess(EntityData data) : base(data)
            {
                loadSceneMode = Data.LoadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single;
            }

            /// <inheritdoc />
            public override void Start()
            {
                if (Data.LoadAsynchronously)
                {
                    return;
                }

                LoadSynchronously();
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                if (Data.LoadAsynchronously)
                {
                    isLoading = true;
                    int sceneIndex = SceneUtility.GetBuildIndexByScenePath(Data.ScenePath);

                    if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
                    {
                        throw new LoadSceneBehaviorException("The provided scene is invalid.");
                    }

                    UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, loadSceneMode);

                    while (asyncLoad.isDone == false)
                    {
                        yield return null;
                    }

                    isLoading = false;
                }
                else
                {
                    yield return null;
                }
            }

            /// <inheritdoc />
            public override void End()
            {
            }

            private void LoadSynchronously()
            {
                int sceneIndex = SceneUtility.GetBuildIndexByScenePath(Data.ScenePath);

                if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    throw new LoadSceneBehaviorException("The provided scene is invalid.");
                }

                SceneManager.LoadScene(sceneIndex, loadSceneMode);
            }

            /// <inheritdoc />
            public override void FastForward()
            {
                if (Data.LoadAsynchronously && isLoading == false)
                {
                    LoadSynchronously();
                }
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }

    /// <summary>
    /// Exception related to load scene behavior.
    /// </summary>
    public class LoadSceneBehaviorException : Exception
    {
        public LoadSceneBehaviorException(string message) : base(message)
        {
        }
    }
}
