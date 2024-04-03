using System;
using UnityEngine;
using VRBuilder.Core;

namespace VRBuilder.Editor.ProcessUpgradeTool
{
    public abstract class Converter<TIn, TOut> : IConverter where TIn : class where TOut : class
    {
        public Type ConvertedType => typeof(TIn);

        protected abstract TOut PerformConversion(TIn oldObject);

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
