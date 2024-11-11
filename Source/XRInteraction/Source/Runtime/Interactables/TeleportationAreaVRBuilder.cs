using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using VRBuilder.Core.Setup;

namespace VRBuilder.XRInteraction.Interactables
{
    /// <summary>
    /// Teleportation area override that ensures a teleport provider is found even when the rig
    /// has been spawned after loading the scene.
    /// </summary>
    [AddComponentMenu("VR Builder/Interactables/Teleportation Area (VR Builder)")]
    public class TeleportationAreaVRBuilder : TeleportationArea, ILayerConfigurator
    {
        /// <inheritdoc />
        public LayerSet LayerSet => LayerSet.Teleportation;

        protected override void Reset()
        {
            base.Reset();

            teleportTrigger = TeleportTrigger.OnSelectExited;
        }

        /// <inheritdoc />
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            CheckTeleportationProvider(args.interactorObject);
            base.OnHoverEntered(args);
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            CheckTeleportationProvider(args.interactorObject);

            base.OnSelectEntered(args);
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            CheckTeleportationProvider(args.interactorObject);

            base.OnSelectExited(args);
        }

        /// <inheritdoc />
        protected override void OnActivated(ActivateEventArgs args)
        {
            CheckTeleportationProvider(args.interactorObject);

            base.OnActivated(args);
        }

        /// <inheritdoc />
        protected override void OnDeactivated(DeactivateEventArgs args)
        {
            CheckTeleportationProvider(args.interactorObject);

            base.OnDeactivated(args);
        }

        /// <inheritdoc />
        public void ConfigureLayers(string interactionLayerName, string raycastLayerName)
        {
            InteractionLayerMask teleportLayer = InteractionLayerMask.NameToLayer(interactionLayerName);
            LayerMask teleportRaycastLayer = LayerMask.NameToLayer(raycastLayerName);

            gameObject.layer = teleportRaycastLayer;
            interactionLayers = 1 << teleportLayer.value;
        }

        private void CheckTeleportationProvider(IXRInteractor interactor)
        {
            if (teleportationProvider != null)
            {
                return;
            }

            TeleportationProvider provider = interactor.transform.GetComponentInParent<TeleportationProvider>();

            if (provider != null)
            {
                teleportationProvider = provider;
            }
        }
    }
}