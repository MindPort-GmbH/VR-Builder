// Copyright (c) 2013-2019 Innoactive GmbH
// Modifications copyright (c) 2021-2026 MindPort GmbH
// Modifications copyright (c) 2026 Aron Schaub
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VRBuilder.Core.Exceptions;

namespace VRBuilder.Core.Configuration.Modes
{
    /// <summary>
    /// Simple mode handler for managing current mode and mode changing.
    /// </summary>
    public sealed class BaseModeHandler : IModeHandler
    {
        /// <inheritdoc />
        public event EventHandler<ModeChangedEventArgs> ModeChanged;

        /// <inheritdoc />
        public int CurrentModeIndex { get; private set; } = 0;

        /// <inheritdoc />
        public IMode CurrentMode
        {
            get => AvailableModes[CurrentModeIndex];
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IMode> AvailableModes { private set; get; }

        public BaseModeHandler(List<IMode> modes = null, int defaultMode = 0)
        {
            AvailableModes = new ReadOnlyCollection<IMode>(modes ?? new List<IMode> { new Mode("Default", new WhitelistTypeRule<IOptional>()) });
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

        /// <inheritdoc />
        public void SetModes(List<IMode> modes, int index)
        {
            AvailableModes = modes.AsReadOnly();
            SetMode(index);
        }
    }
}
