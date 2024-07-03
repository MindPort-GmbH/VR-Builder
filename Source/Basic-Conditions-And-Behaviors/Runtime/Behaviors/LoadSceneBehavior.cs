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
    /// Loading a scene not additively ends the current process.
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
            [DataMember]
            [UsesSpecificProcessDrawer("SceneDropdownDrawer")]
            [DisplayName("Scene to load")]
            public int SceneIndex { get; set; }

            [DataMember]
            [DisplayName("Load additively")]
            public bool LoadAdditively { get; set; }

            [DataMember]
            [DisplayName("Load asynchronously")]
            public bool LoadAsynchronously { get; set; }

            public Metadata Metadata { get; set; }

            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string sceneName = "[NULL]";

                    if (SceneIndex >= 0 && SceneIndex < SceneManager.sceneCountInBuildSettings)
                    {
                        sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(SceneIndex));
                    }

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

            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                if (Data.LoadAsynchronously)
                {
                    isLoading = true;
                    LoadSceneMode loadSceneMode = Data.LoadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single;

                    if (Data.SceneIndex < 0 || Data.SceneIndex >= SceneManager.sceneCountInBuildSettings)
                    {
                        throw new LoadSceneBehaviorException("The provided scene is invalid.");
                    }

                    UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Data.SceneIndex, loadSceneMode);

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
                if (Data.LoadAsynchronously)
                {
                    return;
                }

                LoadSynchronously();
            }

            private void LoadSynchronously()
            {
                LoadSceneMode loadSceneMode = Data.LoadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single;

                if (Data.SceneIndex < 0 || Data.SceneIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    throw new LoadSceneBehaviorException("The provided scene is invalid.");
                }

                SceneManager.LoadScene(Data.SceneIndex, loadSceneMode);
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
