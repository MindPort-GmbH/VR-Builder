// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils.Logging;

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

        protected List<IStepData> unlockers = new List<IStepData>();

        /// <summary>
        /// Decides if the property will be locked when the parent scene object is locked.
        /// </summary>
        public bool LockOnParentObjectLock
        {
            get => lockOnParentObjectLock;
            set => lockOnParentObjectLock = value;
        }

        /// <inheritdoc/>
        public virtual bool IsLocked { get; protected set; }

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

        protected void EmitLocked(bool isLocked)
        {
            Locked?.Invoke(this, new LockStateChangedEventArgs(isLocked));
        }

        protected void EmitUnlocked(bool isLocked)
        {
            Unlocked?.Invoke(this, new LockStateChangedEventArgs(isLocked));
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
                EmitLocked(IsLocked);
            }
            else
            {
                EmitUnlocked(IsLocked);
            }
        }

        /// <inheritdoc/>
        public virtual void RequestLocked(bool lockState, IStepData stepData = null)
        {
            if (lockState == false && stepData != null && unlockers.Contains(stepData) == false)
            {
                unlockers.Add(stepData);
            }

            if (lockState && stepData != null && unlockers.Contains(stepData))
            {
                unlockers.Remove(stepData);
            }

            unlockers.RemoveAll(unlocker => unlocker == null);

            bool canLock = unlockers.Count == 0;

            LogLockState(lockState, stepData, canLock);

            SetLocked(lockState && canLock);
        }

        protected void LogLockState(bool lockState, IStepData stepData, bool canLock)
        {
            if (LifeCycleLoggingConfig.Instance.LogLockState)
            {
                string lockType = lockState ? "lock" : "unlock";
                string requester = stepData == null ? "NULL" : stepData.Name;
                StringBuilder unlockerList = new StringBuilder();

                foreach (IStepData unlocker in unlockers)
                {
                    unlockerList.Append($"\n<i>{unlocker.Name}</i>");
                }

                string listUnlockers = unlockers.Count == 0 ? "" : $"\nSteps keeping this property unlocked:{unlockerList}";

                Debug.Log($"<i>{this.GetType().Name}</i> on <i>{gameObject.name}</i> received a <b>{lockType}</b> request from <i>{requester}</i>." +
                    $"\nCurrent lock state: <b>{IsLocked}</b>. Future lock state: <b>{lockState && canLock}</b>{listUnlockers}");
            }
        }

        /// <inheritdoc/>
        public bool RemoveUnlocker(IStepData data)
        {
            return unlockers.Remove(data);
        }

        protected virtual void HandleObjectUnlocked(object sender, LockStateChangedEventArgs e)
        {
            if (LockOnParentObjectLock && IsLocked)
            {
                SetLocked(false);
            }
        }

        protected virtual void HandleObjectLocked(object sender, LockStateChangedEventArgs e)
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
