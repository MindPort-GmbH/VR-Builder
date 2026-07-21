// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Process scene object property base class. this basically just provides the SceneObject that will have the GUID unique identifier.
    /// </summary>
    [RequireComponent(typeof(ProcessSceneObject))]
    public abstract class ProcessSceneObjectProperty : MonoBehaviour, ISceneObjectProperty
    {
        private ISceneObject sceneObject;

        public ISceneObject SceneObject
        {
            get
            {
                if (sceneObject == null) sceneObject = GetComponent<ISceneObject>();
                return sceneObject;
            }
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void Reset()
        {
            this.AddProcessPropertyExtensions();
        }

        public override string ToString()
        {
            return $"{sceneObject}";
        }
    }
}