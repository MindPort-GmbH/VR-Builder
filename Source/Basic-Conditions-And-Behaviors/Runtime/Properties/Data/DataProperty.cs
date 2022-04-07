using System;
using UnityEngine;
using VRBuilder.Core.Utils.Logging;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Base implementation for process data properties.
    /// </summary>    
    [DisallowMultipleComponent]
    public abstract class DataProperty<T> : ProcessSceneObjectProperty, IDataProperty<T>
    {
        /// <summary>
        /// Defines a default value for the property.
        /// </summary>
        public abstract T DefaultValue { get; }

        /// <inheritdoc/>
        protected T storedValue;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ValueChanged;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ValueReset;

        private void Awake()
        {
            ResetValue();
        }

        /// <inheritdoc/>
        public T GetValue()
        {
            return storedValue;
        }

        /// <inheritdoc/>
        public void ResetValue()
        {
            SetValue(DefaultValue);
            ValueReset?.Invoke(this, EventArgs.Empty); 
        }

        /// <inheritdoc/>
        public void SetValue(T value)
        {
            if((storedValue == null && value == null) || value.Equals(storedValue))
            {
                return;
            }

            if(LifeCycleLoggingConfig.Instance.LogDataPropertyChanges)
            {
                Debug.Log($"{GetType().Name} on '{SceneObject.UniqueName}' changed from {ValueToString(storedValue)} to {ValueToString(value)}.");
            }

            storedValue = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual string ValueToString(T value)
        {
            return value.ToString();
        }
    }
}
