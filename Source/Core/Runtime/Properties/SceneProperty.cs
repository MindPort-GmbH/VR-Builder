// Copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using UnityEngine.SceneManagement;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    public class SceneProperty : MonoBehaviour, ISceneProperty
    {
        public ISceneObject SceneObject { get; }

        public void LoadSynchronously(string scenePath, bool loadAdditively)
        {
            var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                throw new LoadSceneBehaviorException("The provided scene is invalid.");
            }

            SceneManager.LoadScene(sceneIndex, loadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }

        public IAsyncCallback StartLoadAsync(string scenePath, bool loadAdditively)
        {
            var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

            if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                throw new LoadSceneBehaviorException("The provided scene is invalid.");
            }

            return new AsyncCallback(SceneManager.LoadSceneAsync(sceneIndex, loadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single));
        }
    }

    class AsyncCallback : IAsyncCallback
    {
        private readonly AsyncOperation asyncLoad;

        public AsyncCallback(AsyncOperation asyncLoad)
        {
            this.asyncLoad = asyncLoad;
        }

        public bool isDone => asyncLoad.isDone;
    }
}