using System;
using UnityEngine;
using UnityEngine.Events;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;

namespace VRBuilder.BasicInteraction.Properties
{
    public interface ISnapZoneProperty : ISceneObjectProperty, ILockable
    {
        /// <summary>
        /// Called when an object is snapped to the snap zone.
        /// </summary>
        UnityEvent<SnapZonePropertyEventArgs> ObjectAttached { get; }

        /// <summary>
        /// Called when an object is unsnapped from the snap zone.
        /// </summary>
        UnityEvent<SnapZonePropertyEventArgs> ObjectDetached { get; }

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

    public class SnapZonePropertyEventArgs : EventArgs
    {
    }
}