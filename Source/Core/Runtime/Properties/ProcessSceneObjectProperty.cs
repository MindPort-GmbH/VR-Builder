// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    [RequireComponent(typeof(ProcessSceneObject))]
    public abstract class ProcessSceneObjectProperty : MonoBehaviour, ISceneObjectProperty
    {
        private ISceneObject sceneObject;

        public ISceneObject SceneObject
        {
            get
            {
                if (sceneObject == null)
                {
                    sceneObject = GetComponent<ISceneObject>();
                }

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
            return SceneObject.GameObject.name;
        }
    }
}
