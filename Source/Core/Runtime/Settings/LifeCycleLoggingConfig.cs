// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using UnityEngine;
using VRBuilder.Core.Settings;

namespace VRBuilder.Core.Utils.Logging
{
    /// <summary>
    /// ScriptableObject which allows you to configure what of the process life cycle should be logged.
    /// </summary>
    public class LifeCycleLoggingConfig : SettingsObject<LifeCycleLoggingConfig>, ILifeCycleLoggingConfiguration
    {
        /// <summary>
        /// True, if behaviors are allowed to be logged.
        /// </summary>
        [SerializeField]
        private bool logBehaviors = false;

        /// <summary>
        /// True, if conditions are allowed to be logged.
        /// </summary>
        [SerializeField]
        private bool logConditions = false;

        /// <summary>
        /// True, if chapters are allowed to be logged.
        /// </summary>
        [SerializeField]
        private bool logChapters = true;

        /// <summary>
        /// True, if steps are allowed to be logged.
        /// </summary>
        [SerializeField]
        private bool logSteps = true;

        /// <summary>
        /// True, if transitions are allowed to be logged.
        /// </summary>
        [SerializeField]
        private bool logTransitions = false;

        /// <summary>
        /// True, if data property changes are allowed to be logged.
        /// </summary>
        [SerializeField]
        private bool logDataPropertyChanges = false;

        /// <summary>
        /// True, if verbose logging is enabled.
        /// </summary>
        [SerializeField]
        public bool logLockState = false;

        public bool LogBehaviors
        {
            get => logBehaviors;
            set => logBehaviors = value;
        }

        public bool LogConditions
        {
            get => logConditions;
            set => logConditions = value;
        }

        public bool LogChapters
        {
            get => logChapters;
            set => logChapters = value;
        }

        public bool LogSteps
        {
            get => logSteps;
            set => logSteps = value;
        }

        public bool LogTransitions
        {
            get => logTransitions;
            set => logTransitions = value;
        }

        public bool LogDataPropertyChanges
        {
            get => logDataPropertyChanges;
            set => logDataPropertyChanges = value;
        }

        public bool LogLockState
        {
            get => logLockState;
            set => logLockState = value;
        }
    }
}