using UnityEngine;
using VRBuilder.Core.Configuration.Modes;

namespace VRBuilder.Core.EntityOwners.ParallelEntityCollection
{
    /// <summary>
    /// A process over a collection of entities which aborts them at the same time, in parallel.
    /// </summary>
    internal class ParallelAbortingProcess<TCollectionData> : InstantProcess<TCollectionData> where TCollectionData : class, IEntityCollectionData, IModeData
    {
        public ParallelAbortingProcess(TCollectionData data) : base(data)
        {
        }

        /// <inheritdoc />
        public override void Start()
        {
            foreach (IEntity child in Data.GetChildren())
            {
                Debug.Log($"Aborting ´{child}");
                child.LifeCycle.Abort();
            }
        }
    }
}
