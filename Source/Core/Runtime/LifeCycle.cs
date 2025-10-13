// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Exceptions;

namespace VRBuilder.Core
{
    /// <summary>
    /// The implementation of the <seealso cref="ILifeCycle"/> interface.
    /// </summary>
    public sealed class LifeCycle : ILifeCycle
    {
        private bool deactivateAfterActivation;
        private IEnumerator update;
        private IStageProcess process;

        private bool IsCurrentStageProcessFinished => update == null;

        private readonly Dictionary<Stage, bool> fastForwardedStates = new Dictionary<Stage, bool>
        {
            { Stage.Inactive, false },
            { Stage.Activating, false },
            { Stage.Active, false },
            { Stage.Deactivating, false },
            { Stage.Aborting, false },
        };

        private IEntity Owner { get; set; }

        public LifeCycle(IEntity owner)
        {
            Stage = Stage.Inactive;
            Owner = owner;
        }

        ///<inheritdoc />
        public event EventHandler<ActivationStateChangedEventArgs> StageChanged;

        ///<inheritdoc />
        public Stage Stage { get; private set; }

        ///<inheritdoc />
        public void Activate()
        {
            if (Stage != Stage.Inactive)
            {
                throw new InvalidStateException("Process entity can only be activated when not running yet. Stage: " + Stage);
            }

            StartActivating();
        }

        ///<inheritdoc />
        public void Deactivate()
        {
            if (Stage == Stage.Activating)
            {
                // Deactivate is called while activation is still running - this is valid, but
                // the actual deactivation has to be delayed until the activation is finished.
                deactivateAfterActivation = true;
            }
            else if (Stage != Stage.Active)
            {
                throw new InvalidStateException("Process entity can only be deactivated when already running");
            }
            else
            {
                StartDeactivating();
            }
        }

        /// <inheritdoc/>
        public void Abort()
        {
            if (Stage == Stage.Inactive)
            {
                throw new InvalidStateException("Process entity can only be aborted when already running.");
            }

            if (Stage == Stage.Aborting)
            {
                throw new InvalidStateException("Attempted to abort process entity which is already aborting.");
            }

            StartAborting();
        }

        ///<inheritdoc />
        public void MarkToFastForward()
        {
            fastForwardedStates[Stage.Deactivating] = true;

            if (Stage == Stage.Deactivating)
            {
                FastForward();
                return;
            }

            fastForwardedStates[Stage.Active] = true;

            if (Stage == Stage.Active)
            {
                FastForward();
                return;
            }

            fastForwardedStates[Stage.Activating] = true;

            FastForward();
        }

        ///<inheritdoc />
        public void MarkToFastForwardStage(Stage stage)
        {
            if (stage == Stage.Inactive)
            {
                return;
            }

            fastForwardedStates[stage] = true;

            if (stage == Stage)
            {
                FastForward();
            }
        }

        ///<inheritdoc />
        public void Update()
        {
            if (IsCurrentStageProcessFinished)
            {
                return;
            }

            try
            {
                if (update.MoveNext() == false)
                {
                    FinishCurrentState();
                }
            }
            catch (Exception exception)
            {
                LogException(exception, "Update");
            }
        }

        private void FastForward()
        {
            if (IsCurrentStageProcessFinished)
            {
                return;
            }

            try
            {
                process.FastForward();
            }
            catch (Exception exception)
            {
                LogException(exception, "FastForward");
            }

            FinishCurrentState();
        }

        private void FinishCurrentState()
        {
            update = null;

            try
            {
                process.End();
            }
            catch (Exception exception)
            {
                LogException(exception, "End");
            }

            fastForwardedStates[Stage] = false;

            switch (Stage)
            {
                case Stage.Inactive:
                    return;
                case Stage.Activating:
                    StartActive();
                    return;
                case Stage.Active:
                    return;
                case Stage.Deactivating:
                    StartInactive();
                    return;
                case Stage.Aborting:
                    StartInactive();
                    return;
            }
        }

        private void StartActivating()
        {
            deactivateAfterActivation = false;

            ChangeStage(Stage.Activating);

            if (IsInFastForward)
            {
                FastForward();
            }
        }

        private void StartActive()
        {
            ChangeStage(Stage.Active);

            if (IsInFastForward)
            {
                FastForward();
            }

            if (deactivateAfterActivation)
            {
                Deactivate();
            }
        }

        private void StartDeactivating()
        {
            ChangeStage(Stage.Deactivating);

            if (IsInFastForward)
            {
                FastForward();
            }
        }

        private void StartAborting()
        {
            ChangeStage(Stage.Aborting, false);

            if (IsInFastForward)
            {
                FastForward();
            }
        }

        private void StartInactive()
        {
            ChangeStage(Stage.Inactive);
        }

        private bool IsInFastForward
        {
            get { return fastForwardedStates[Stage]; }
        }

        private void SetCurrentStageProcess()
        {
            switch (Stage)
            {
                case Stage.Inactive:
                    process = new EmptyProcess();
                    break;
                case Stage.Activating:
                    process = Owner.GetActivatingProcess();
                    break;
                case Stage.Active:
                    process = Owner.GetActiveProcess();
                    break;
                case Stage.Deactivating:
                    process = Owner.GetDeactivatingProcess();
                    break;
                case Stage.Aborting:
                    process = Owner.GetAbortingProcess();
                    break;
            }

            update = process.Update();
        }

        private void ChangeStage(Stage stage, bool fastForward = true)
        {
            // Interrupt and fast-forward the current stage process, if it had no time to iterate completely.
            if (fastForward)
            {
                FastForward();
            }

            Stage = stage;
            SetCurrentStageProcess();

            try
            {
                process.Start();
            }
            catch (Exception exception)
            {
                LogException(exception, "Start");
            }

            StageChanged?.Invoke(this, new ActivationStateChangedEventArgs(stage));
        }

        private void LogException(Exception exception, string function)
        {
            string ownerInfo = "";
            string step = "";

            IEntity parentStep = Owner;

            while (parentStep != null && parentStep is IStep == false)
            {
                parentStep = parentStep.Parent;
            }

            if (parentStep != null)
            {
                step = $" <b>Step</b> '<i>{(parentStep as IStep).Data.Name}</i>',";
            }

            if (Owner is IStep == false)
            {
                ownerInfo = $" <b>{Owner.GetType().Name}</b>";
                IDataOwner dataOwner = Owner as IDataOwner;

                if (dataOwner != null && dataOwner.Data is INamedData)
                {
                    ownerInfo += $" '<i>{(dataOwner.Data as INamedData).Name}</i>'";
                }
            }

            Debug.LogError($"Exception in{step}{ownerInfo} while <b>{Stage} ({function})</b>\n{exception}");
        }
    }
}
