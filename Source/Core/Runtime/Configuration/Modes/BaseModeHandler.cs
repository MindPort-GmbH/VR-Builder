// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Configuration.Modes
{
    /// <summary>
    /// Simple mode handler .
    /// </summary>
    public sealed class BaseModeHandler : MonoBehaviour, IModeHandler
    {
        /// <inheritdoc />
        public event EventHandler<ModeChangedEventArgs> ModeChanged;

        /// <inheritdoc />
        public int CurrentModeIndex { get; private set; }

        /// <inheritdoc />
        public IMode CurrentMode
        {
            get => AvailableModes[CurrentModeIndex];
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IMode> AvailableModes { get; }

        public BaseModeHandler(List<IMode> modes, int defaultMode = 0)
        {
            AvailableModes = new ReadOnlyCollection<IMode>(new List<IMode> { ServiceRegistry.Get<IModeService>().ActiveOrDefaultMode });
            CurrentModeIndex = defaultMode;
        }

        /// <inheritdoc />
        public void SetMode(int index)
        {
            if (AvailableModes.Count == 0)
            {
                throw new MissingModeException("You cannot access the current process mode index because there are no process modes available.");
            }

            if (CurrentModeIndex >= AvailableModes.Count)
            {
                string message = $"The current process mode index is set to {CurrentModeIndex} but the current number of available process modes is {AvailableModes.Count}.";
                throw new IndexOutOfRangeException(message);
            }

            CurrentModeIndex = index;

            ModeChanged?.Invoke(this, new ModeChangedEventArgs(CurrentMode));
        }

        /// <inheritdoc />
        public void SetMode(IMode mode)
        {
            if (AvailableModes.Contains(mode))
            {
                SetMode(AvailableModes.IndexOf(mode));
            }
            else
            {
                throw new MissingModeException("Given mode is not part of the available modes!");
            }
        }
    }
}
