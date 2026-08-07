// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using UnityEngine;
using VRBuilder.Core.Runtime.Registry;

namespace VRBuilder.Core.Input
{
    /// <summary>
    /// Base class for InputActionListener.
    /// </summary>
    public abstract class InputActionListener : MonoBehaviour, IInputActionListener
    {
        /// <inheritdoc/>
        public virtual int Priority { get; } = 1000;

        /// <inheritdoc/>
        public virtual bool IgnoreFocus { get; } = false;

        /// <summary>
        /// Registers the given method as input event, the name of the method will be the event name.
        /// </summary>
        protected virtual void RegisterInputEvent(Action<InputController.InputEventArgs> action)
        {
            ServiceRegistry.Get<InputController>().RegisterEvent(this, action);
        }

        /// <summary>
        /// Unregisters the given method as input event, the name of the method will be the event name.
        /// </summary>
        protected virtual void UnregisterInputEvent(Action<InputController.InputEventArgs> action)
        {
            ServiceRegistry.Get<InputController>().RegisterEvent(this, action);
        }
    }
}
