using UnityEngine;

namespace VRBuilder.Core.SceneObjects
{
    /// <summary>
    /// References parts of a XR Rig.
    /// </summary>
    public interface IXRRigTransform
    {
        /// <summary>
        /// Returns transform of the head of the rig.
        /// </summary>
        Transform Head { get; }

        /// <summary>
        /// Returns transform of the left hand.
        /// </summary>
        Transform LeftHand { get; }

        /// <summary>
        /// Returns transform of the right hand.
        /// </summary>
        Transform RightHand { get; }

        /// <summary>
        /// Returns transform of the base of the rig.
        /// </summary>
        Transform Base { get; }
    }
}