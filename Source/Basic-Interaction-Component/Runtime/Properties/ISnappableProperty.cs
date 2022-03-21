using System;
using VRBuilder.Core.Properties;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface ISnappableProperty : ISceneObjectProperty
    {
        event EventHandler<EventArgs> Snapped;
        event EventHandler<EventArgs> Unsnapped;

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
}