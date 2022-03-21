using System;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface IGrabbableProperty : ISceneObjectProperty, ILockable
    {
        event EventHandler<EventArgs> Grabbed;
        event EventHandler<EventArgs> Ungrabbed;
        
        /// <summary>
        /// Is object currently grabbed.
        /// </summary>
        bool IsGrabbed { get; }
        
        /// <summary>
        /// Instantaneously simulate that the object was grabbed.
        /// </summary>
        void FastForwardGrab();

        /// <summary>
        /// Instantaneously simulate that the object was ungrabbed.
        /// </summary>
        void FastForwardUngrab();
    }
}
