using System;
using UnityEngine;
using VRBuilder.Core;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    /// <summary>
    /// Generic implementation of <see cref="IConverter"/>.
    /// </summary>
    public abstract class Converter<TIn, TOut> : IConverter where TIn : class where TOut : class
    {
        /// <inheritdoc/>
        public Type ConvertedType => typeof(TIn);

        /// <summary>
        /// Returns an object which can replace the provided object.
        /// </summary>
        protected abstract TOut PerformConversion(TIn oldObject);

        /// <inheritdoc/>
        public object Convert(object oldObject)
        {
            TOut newObject = PerformConversion((TIn)oldObject);
            Debug.Log($"Replaced obsolete <i>{typeof(TIn).Name}</i> '<b>{GetObjectName(oldObject)}</b>' with <i>{typeof(TOut).Name}</i> '<b>{GetObjectName(newObject)}</b>'.");
            return newObject;
        }

        private string GetObjectName(object obj)
        {
            if (obj is INamedData namedData)
            {
                return namedData.Name;
            }

            if (obj is IDataOwner dataOwner && dataOwner.Data is INamedData data)
            {
                return data.Name;
            }

            return obj.ToString();
        }
    }
}
