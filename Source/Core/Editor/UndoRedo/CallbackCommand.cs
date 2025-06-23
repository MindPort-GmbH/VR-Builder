// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;

namespace VRBuilder.Core.Editor.UndoRedo
{
    /// <summary>
    /// A revertable command which defines Do/Undo logic through callbacks.
    /// </summary>
    public class CallbackCommand : IRevertableCommand
    {
        private readonly Action doCallback;
        private readonly Action undoCallback;

        public CallbackCommand(Action doCallback, Action undoCallback)
        {
            this.doCallback = doCallback;
            this.undoCallback = undoCallback;
        }

        /// <inheritdoc />
        public virtual void Do()
        {
            doCallback.Invoke();
        }

        /// <inheritdoc />
        public virtual void Undo()
        {
            undoCallback.Invoke();
        }
    }
}
