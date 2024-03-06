// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Collections.Generic;

namespace VRBuilder.Core.Utils
{
    /// <summary>
    /// Class for delayed execution of Action delegates.
    /// </summary>
    public class DelayedExecutionQueue
    {
        private List<Action> firstPassQueue = new List<Action>();

        private List<Action> secondPassQueue = new List<Action>();

        /// <summary>
        /// Adds an Action to the end of the first execution queue.
        /// </summary>
        /// <param name="action"></param>
        public void AddFirstPassAction(Action action)
        {
            firstPassQueue.Add(action);
        }

        /// <summary>
        /// Adds an Action to the end of the second execution queue.
        /// </summary>
        /// <param name="action"></param>
        public void AddSecondPassAction(Action action)
        {
            secondPassQueue.Add(action);
        }

        /// <summary>
        /// Invokes every Action in the first queue and then in the second.
        /// </summary>
        public void Execute()
        {
            foreach (Action action in firstPassQueue)
            {
                action.Invoke();
            }

            foreach (Action action in secondPassQueue)
            {
                action.Invoke();
            }
        }
    }
}
