using System;
using System.Collections.Generic;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// Event args for guid container events.
    /// </summary>
    public class GuidContainerEventArgs : EventArgs
    {
        public readonly Guid Guid;

        public GuidContainerEventArgs(Guid guid)
        {
            Guid = guid;
        }
    }

    /// <summary>
    /// A container for a list of guids that are associated to an object.
    /// </summary>
    public interface IGuidContainer
    {
        /// <summary>
        /// Raised when a guid is added.
        /// </summary>
        event EventHandler<GuidContainerEventArgs> GuidAdded;

        /// <summary>
        /// Raised when a guid is removed.
        /// </summary>
        event EventHandler<GuidContainerEventArgs> GuidRemoved;

        /// <summary>
        /// All guids on the object.
        /// </summary>
        IEnumerable<Guid> Guids { get; }

        /// <summary>
        /// True if the object has the specified guid.
        /// </summary>
        bool HasGuid(Guid guid);

        /// <summary>
        /// Add the specified guid.
        /// </summary>        
        void AddGuid(Guid guid);

        /// <summary>
        /// Remove the specified guid.
        /// </summary>
        bool RemoveGuid(Guid guid);
    }
}
