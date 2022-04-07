using System;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Properties;
using UnityEngine;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface ISnapZoneProperty : ISceneObjectProperty, ILockable
    {
        event EventHandler<EventArgs> ObjectSnapped;
        event EventHandler<EventArgs> ObjectUnsnapped;
        
        /// <summary>
        /// Currently has an object snapped into this snap zone.
        /// </summary>
        bool IsObjectSnapped { get; }
        
        /// <summary>
        /// Object which is snapped into this snap zone.
        /// </summary>
        ISnappableProperty SnappedObject { get; set; }
        
        /// <summary>
        /// Snap zone object.
        /// </summary>
        GameObject SnapZoneObject { get; }
        
        // TODO: Probably make a IConfigurable interface for modes
        void Configure(IMode mode);
    }
}