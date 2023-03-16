using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;
using System;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface ITouchableProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when touched.
        /// </summary>
        event EventHandler<EventArgs> Touched;

        /// <summary>
        /// Called when stopped touching.
        /// </summary>
        event EventHandler<EventArgs> Untouched;

        /// <summary>
        /// Is object currently touched.
        /// </summary>
        bool IsBeingTouched { get; }
        
        /// <summary>
        /// Instantaneously simulate that the object was touched.
        /// </summary>
        void FastForwardTouch();

        /// <summary>
        /// Force this property to a specified touched state.
        /// </summary>   
        void ForceSetTouched(bool isTouched);
    }
}