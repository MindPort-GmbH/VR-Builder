// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

﻿using System;
 using VRBuilder.Core.SceneObjects;
 using UnityEngine;
using System.Collections.Generic;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// <see cref="ProcessSceneObjectProperty"/> which is lockable, to allow the restrictive environment to handle
    /// locking/unlocking your properties, extend this class.
    /// </summary>
    public abstract class LockableProperty : ProcessSceneObjectProperty, ILockable
    {
        ///  <inheritdoc/>
        public event EventHandler<LockStateChangedEventArgs> Locked;
        ///  <inheritdoc/>
        public event EventHandler<LockStateChangedEventArgs> Unlocked;

        [SerializeField]
        private bool lockOnParentObjectLock = true;

        private List<IStepData> unlockers = new List<IStepData>();

        /// <summary>
        /// Decides if the property will be locked when the parent scene object is locked.
        /// </summary>
        public bool LockOnParentObjectLock
        {
            get => lockOnParentObjectLock;
            set => lockOnParentObjectLock = value;
        }

        /// <inheritdoc/>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// On default the lockable property will use this value to determine if its locked at the end of a step.
        /// </summary>
        public virtual bool EndStepLocked { get; } = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            SceneObject.Locked += HandleObjectLocked;
            SceneObject.Unlocked += HandleObjectUnlocked;
        }

        protected virtual void OnDisable()
        {
            SceneObject.Locked -= HandleObjectLocked;
            SceneObject.Unlocked -= HandleObjectUnlocked;
        }

        /// <inheritdoc/>
        public virtual void SetLocked(bool lockState)
        {
            if (IsLocked == lockState)
            {
                return;
            }

            IsLocked = lockState;

            InternalSetLocked(lockState);

            if (IsLocked)
            {
                if (Locked != null)
                {
                    Locked.Invoke(this, new LockStateChangedEventArgs(IsLocked));
                }
            }
            else
            {
                if (Unlocked != null)
                {
                    Unlocked.Invoke(this, new LockStateChangedEventArgs(IsLocked));
                }
            }
        }

        /// <inheritdoc/>
        public virtual void RequestLocked(bool lockState, IStepData stepData = null)
        {
            if(lockState && stepData != null && unlockers.Contains(stepData) == false)
            {
                unlockers.Add(stepData);
            }

            if(lockState == false && stepData != null && unlockers.Contains(stepData))
            {
                unlockers.Remove(stepData);
            }

            bool canLock = unlockers.Count == 0;

            string lockType = lockState ? "lock" : "unlock";
            string requester = stepData == null ? "NULL" : stepData.Name;
            Debug.Log($"{this.GetType().Name} received a {lockType} request from {requester}. Current lock state: {IsLocked}. Future lock state: {lockState && canLock}");

            SetLocked(lockState && canLock);
        }

        private void HandleObjectUnlocked(object sender, LockStateChangedEventArgs e)
        {
            if (LockOnParentObjectLock && IsLocked)
            {
                SetLocked(false);
            }
        }

        private void HandleObjectLocked(object sender, LockStateChangedEventArgs e)
        {
            if (LockOnParentObjectLock && IsLocked == false)
            {
                SetLocked(true);
            }
        }

        /// <summary>
        /// Handle your internal locking affairs here.
        /// </summary>
        protected abstract void InternalSetLocked(bool lockState);
    }
}
