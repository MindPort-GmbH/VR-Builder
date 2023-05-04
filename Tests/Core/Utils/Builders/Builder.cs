// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using VRBuilder.Core.Utils;

namespace VRBuilder.Tests.Builder
{
    /// <summary>
    /// Abstract Builder pattern class.
    /// An actual resulting object configuration is made through delayed execution of Actions.
    /// This way, it guarantees that with every Build() call a completely new instance will be created.
    /// </summary>
    /// <typeparam name="T">Type of an object to build.</typeparam>
    public abstract class Builder<T>
    {
        /// <summary>
        /// Object that is currently being built or was built during the last Build() call.
        /// </summary>
        protected T Result { get; set; }

        private DelayedExecutionQueue executionQueue = new DelayedExecutionQueue();

        /// <summary>
        /// Add an Action to a first execution pass.
        /// Typically, you want to add Actions which have to be called prior result object creation through this method.
        /// </summary>
        /// <param name="action">An Action to be executed in a first pass.</param>
        protected void AddFirstPassAction(Action action)
        {
            executionQueue.AddFirstPassAction(action);
        }

        /// <summary>
        /// Add an Action to a second execution pass.
        /// Typically, you want to add instantiation of an object there first (Builder constructor is a good place to call this method).
        /// Then, add everything that has to be executed after the object is created.
        /// </summary>
        /// <param name="action">An Action to be executed in a second pass.</param>
        protected void AddSecondPassAction(Action action)
        {
            executionQueue.AddSecondPassAction(action);
        }

        /// <summary>
        /// Build an object and return it.
        /// Calls Cleanup() prior to execution of stored Actions.
        /// </summary>
        /// <returns>Resulting object that had been built.</returns>
        public virtual T Build()
        {
            Cleanup();
            executionQueue.Execute();
            return Result;
        }

        /// <summary>
        /// Clear changes you've made during previous Build() call there.
        /// For example, nullify previous result or clear some buffer variables.
        /// </summary>
        protected virtual void Cleanup()
        {
            Result = default(T);
        }
    }
}
