// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using UnityEngine;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Used to identify the user within the scene.
    /// </summary>
    public class UserSceneObject : ProcessSceneObject
    {
        [SerializeField]
        protected AudioSource processAudioSourceOverride = null;
        protected AudioSource processAudioSource = null;

        /// <summary>
        /// Returns the audio source used to play process audio to this user.
        /// </summary>
        public AudioSource ProcessAudioSource
        {            
            get
            {
                if (processAudioSourceOverride != null)
                {
                    return processAudioSourceOverride;
                }

                if(processAudioSource == null)
                {
                    processAudioSource = GetComponent<AudioSource>();
                }

                if(processAudioSource == null)
                {
                    processAudioSource = gameObject.AddComponent<AudioSource>();
                }

                return processAudioSource;
            }            
        }

        protected new void Awake()
        {
            base.Awake();
            uniqueName = "User";
        }
    }
}
