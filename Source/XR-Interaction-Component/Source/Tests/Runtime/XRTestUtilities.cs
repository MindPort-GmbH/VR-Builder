using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

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
        public static XRRig CreateXRRig()
        {
            GameObject xrRigGO = new GameObject();
            xrRigGO.name = "XR Rig";
            XRRig xrRig = xrRigGO.AddComponent<XRRig>();

            // add camera offset
            GameObject cameraOffsetGO = new GameObject();
            cameraOffsetGO.name = "CameraOffset";
            cameraOffsetGO.transform.SetParent(xrRig.transform,false);
            xrRig.cameraFloorOffsetObject = cameraOffsetGO;

            xrRig.transform.position = Vector3.zero;
            xrRig.transform.rotation = Quaternion.identity;

            // camera and track pose driver
            GameObject cameraGO = new GameObject();
            cameraGO.name = "Camera";
            Camera camera = cameraGO.AddComponent<Camera>();

            cameraGO.transform.SetParent(cameraOffsetGO.transform, false);
            xrRig.cameraGameObject = cameraGO;

            XRDevice.DisableAutoXRCameraTracking(camera, true);

            LocomotionSystem locomotionSystem = xrRigGO.AddComponent<LocomotionSystem>();
            TeleportationProvider teleportationProvider = xrRigGO.AddComponent<TeleportationProvider>();

            locomotionSystem.xrRig = xrRig;
            teleportationProvider.system = locomotionSystem;

            return xrRig;
        }

        /// <summary>
        /// Creates a new DirectInteractor.
        /// </summary>
        public static DirectInteractor CreateDirectInteractor()
        {
            GameObject interactorGO = new GameObject("XR Interactor");
            CreateGOSphereCollider(interactorGO);
            DirectInteractor interactor = interactorGO.AddComponent<DirectInteractor>();
            XRController controller = interactorGO.GetComponent<XRController>();
            controller.enableInputTracking = false;
            
            return interactor;
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
        public static XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject("XR Socket Interactor");
            CreateGOSphereCollider(interactorGO);
            return interactorGO.AddComponent<XRSocketInteractor>();
        }

        private static void CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
        }
    }
}