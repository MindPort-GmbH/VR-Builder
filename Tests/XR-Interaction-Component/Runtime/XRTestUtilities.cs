using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRBuilder.XRInteraction.Tests
{
    /// <summary>
    /// Utility class for generatating mock interaction actors.
    /// </summary>
    public static class XRTestUtilities
    {
        /// <summary>
        /// Creates a new XRInteractionManager.
        /// </summary>
        public static XRInteractionManager CreateInteractionManager()
        {
            GameObject manager = new GameObject("XR Interaction Manager");
            return manager.AddComponent<XRInteractionManager>();
        }

        /// <summary>
        /// Creates a new XRRig.
        /// </summary>
        public static XROrigin CreateXRRig()
        {
            GameObject xrRigGO = new GameObject();
            xrRigGO.name = "XR Rig";
            XROrigin xrRig = xrRigGO.AddComponent<XROrigin>();

            // add camera offset
            GameObject cameraOffsetGO = new GameObject();
            cameraOffsetGO.name = "CameraOffset";
            cameraOffsetGO.transform.SetParent(xrRig.transform, false);
            xrRig.CameraFloorOffsetObject = cameraOffsetGO;

            xrRig.transform.position = Vector3.zero;
            xrRig.transform.rotation = Quaternion.identity;

            // camera and track pose driver
            GameObject cameraGO = new GameObject();
            cameraGO.name = "Camera";
            Camera camera = cameraGO.AddComponent<Camera>();

            cameraGO.transform.SetParent(cameraOffsetGO.transform, false);
            xrRig.Camera = camera;

            XRDevice.DisableAutoXRCameraTracking(camera, true);

            LocomotionSystem locomotionSystem = xrRigGO.AddComponent<LocomotionSystem>();
            UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider = xrRigGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();

            locomotionSystem.xrOrigin = xrRig;
            teleportationProvider.system = locomotionSystem;

            return xrRig;
        }

        /// <summary>
        /// Creates a new InteractableObject.
        /// </summary>
        public static InteractableObject CreateInteractableObjcet()
        {
            GameObject interactableGO = new GameObject("XR Interactable");
            CreateGOSphereCollider(interactableGO, false);
            InteractableObject interactable = interactableGO.AddComponent<InteractableObject>();
            Rigidbody rigidBody = interactableGO.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;

            return interactable;
        }

        /// <summary>
        /// Creates a new XRSocketInteractor.
        /// </summary>
        public static UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject("XR Socket Interactor");
            CreateGOSphereCollider(interactorGO);
            return interactorGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        }

        private static void CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
        }
    }
}