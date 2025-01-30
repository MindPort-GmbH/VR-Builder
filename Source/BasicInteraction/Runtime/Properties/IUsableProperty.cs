using System;
using UnityEngine.Events;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface IUsableProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when the object is used.
        /// </summary>
        UnityEvent<UsablePropertyEventArgs> UseStarted { get; }

        /// <summary>
        /// Called when object use has ended.
        /// </summary>
        UnityEvent<UsablePropertyEventArgs> UseEnded { get; }

        /// <summary>
        /// Is object currently used.
        /// </summary>
        bool IsBeingUsed { get; }

        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        void FastForwardUse();

        /// <summary>
        /// Force this property to a specified use state.
        /// </summary>        
        void ForceSetUsed(bool isUsed);
    }

    public class UsablePropertyEventArgs : EventArgs
    {
    }
}
