using System;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    /// <summary>
    /// Interface for <see cref="ISceneObjectProperty"/>s that can be used for teleport into.
    /// </summary>
    public interface ITeleportationProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Emitted when a teleportation action into this <see cref="ISceneObject"/> was done.
        /// </summary>
        event EventHandler<EventArgs> Teleported;
        
        /// <summary>
        /// True if a teleportation action into this <see cref="ITeleportationProperty"/> was done.
        /// </summary>
        bool WasUsedToTeleport { get; }
        
        /// <summary>
        /// Sets <see cref="WasUsedToTeleport"/> to true.
        /// </summary>
        /// <remarks>
        /// This method is called every time a <see cref="Conditions.TeleportCondition"/> is activate.
        /// </remarks>
        void Initialize();
        
        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        void FastForwardTeleport();
    }
}
