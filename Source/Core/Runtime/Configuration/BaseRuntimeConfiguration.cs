// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2022 MindPort GmbH

using System;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.IO;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Serialization;
using UnityEngine;

namespace VRBuilder.Core.Configuration
{
    /// <summary>
    /// Base class for your runtime process configuration. Extend it to create your own.
    /// </summary>
#pragma warning disable 0618
    public abstract class BaseRuntimeConfiguration : IRuntimeConfiguration
    {
#pragma warning restore 0618
        private ISceneObjectRegistry sceneObjectRegistry;

        /// <inheritdoc />
        public virtual ISceneObjectRegistry SceneObjectRegistry
        {
            get
            {
                if (sceneObjectRegistry == null)
                {
                    sceneObjectRegistry = new SceneObjectRegistry();
                }

                return sceneObjectRegistry;
            }
        }

        /// <inheritdoc />
        public IProcessSerializer Serializer { get; set; } = new ImprovedNewtonsoftJsonProcessSerializer();

        /// <summary>
        /// Default input action asset which is used when no customization of key bindings are done.
        /// Should be stored inside the VR Builder package.
        /// </summary>
        public virtual string DefaultInputActionAssetPath { get; } = "KeyBindings/BuilderDefaultKeyBindings";

        /// <summary>
        /// Custom InputActionAsset path which is used when key bindings are modified.
        /// Should be stored in project path.
        /// </summary>
        public virtual string CustomInputActionAssetPath { get; } = "KeyBindings/BuilderCustomKeyBindings";

#if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_PACKAGE
        private UnityEngine.InputSystem.InputActionAsset inputActionAsset;

        /// <summary>
        /// Current active InputActionAsset.
        /// </summary>
        public virtual UnityEngine.InputSystem.InputActionAsset CurrentInputActionAsset
        {
            get
            {
                if (inputActionAsset == null)
                {
                    inputActionAsset = Resources.Load<UnityEngine.InputSystem.InputActionAsset>(CustomInputActionAssetPath);
                    if (inputActionAsset == null)
                    {
                        inputActionAsset = Resources.Load<UnityEngine.InputSystem.InputActionAsset>(DefaultInputActionAssetPath);
                    }
                }

                return inputActionAsset;
            }

            set => inputActionAsset = value;
        }
#endif

        /// <inheritdoc />
        public IModeHandler Modes { get; protected set; }

        /// <inheritdoc />
        public abstract ProcessSceneObject User { get; }

        /// <inheritdoc />
        public abstract AudioSource InstructionPlayer { get; }

        /// <summary>
        /// Determines the property locking strategy used for this runtime configuration.
        /// </summary>
        public StepLockHandlingStrategy StepLockHandling { get; set; }

        protected BaseRuntimeConfiguration() : this (new DefaultStepLockHandling())
        {
        }

        protected BaseRuntimeConfiguration(StepLockHandlingStrategy lockHandling)
        {
            StepLockHandling = lockHandling;
        }

        /// <inheritdoc />
        public virtual IProcess LoadProcess(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Given path is null or empty!");
            }

            byte[] serialized = FileManager.Read(path);
            return Serializer.ProcessFromByteArray(serialized);
        }
    }
}
