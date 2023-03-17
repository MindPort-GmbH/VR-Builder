using System;
using UnityEngine.Events;
using VRBuilder.Core.Properties;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface ISnappableProperty : ISceneObjectProperty
    {
        event EventHandler<EventArgs> Snapped;
        event EventHandler<EventArgs> Unsnapped;

        UnityEvent<SnappablePropertyEventArgs> OnSnapped { get; }
        UnityEvent<SnappablePropertyEventArgs> OnUnsnapped { get; }

        /// <summary>
        /// Is object currently snapped.
        /// </summary>
        bool IsSnapped { get; }
        
        /// <summary>
        /// Will object be locked when it has been snapped.
        /// </summary>
        bool LockObjectOnSnap { get; }
        
        /// <summary>
        /// Zone to snap into.
        /// </summary>
        ISnapZoneProperty SnappedZone { get; set; }

        /// <summary>
        /// Instantaneously simulate that the object was snapped into given <paramref name="snapZone"/>.
        /// </summary>
        /// <param name="snapZone">Snap zone to snap into.</param>
        void FastForwardSnapInto(ISnapZoneProperty snapZone);
    }

    public class SnappablePropertyEventArgs : EventArgs
    {
        public readonly ISnapZoneProperty SnappedZone;
        public readonly ISnappableProperty SnappedObject;

        public SnappablePropertyEventArgs(ISnappableProperty snappedObject, ISnapZoneProperty snappedZone)
        {
            SnappedObject = snappedObject;
            SnappedZone = snappedZone;
        }
    }
}