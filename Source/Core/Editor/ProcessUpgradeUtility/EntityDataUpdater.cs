using System;
using UnityEngine;
using VRBuilder.Core;

namespace VRBuilder.Editor.Utils
{
    public abstract class EntityDataUpdater<T> : IEntityDataUpdater where T : class, IDataOwner
    {
        public Type SupportedType => typeof(T);

        protected abstract void Update(T dataOwner);

        public void UpdateData(IDataOwner dataOwner)
        {
            T castDataOwner = dataOwner as T;

            if (castDataOwner != null)
            {
                Update(castDataOwner);
            }
            else
            {
                Debug.Log("Invalid data.");
            }
        }
    }
}
