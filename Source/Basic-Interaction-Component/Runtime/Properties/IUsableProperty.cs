using System;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface IUsableProperty : ISceneObjectProperty, ILockable
    {
        event EventHandler<EventArgs> UsageStarted;
        event EventHandler<EventArgs> UsageStopped;
        
        /// <summary>
        /// Is object currently used.
        /// </summary>
        bool IsBeingUsed { get; }
        
        /// <summary>
        /// Instantaneously simulate that the object was used.
        /// </summary>
        void FastForwardUse();
        
    }
}
