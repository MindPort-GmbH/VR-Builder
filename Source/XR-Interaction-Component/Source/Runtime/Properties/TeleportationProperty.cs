using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.BasicInteraction.Properties;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ITeleportationProperty"/>.
    /// </summary>
    /// <remarks>
    /// This implementation is based on 'TeleportationAnchor'.
    /// </remarks>
    [RequireComponent(typeof(TeleportationAnchor), typeof(BoxCollider))]
    public class TeleportationProperty : LockableProperty, ITeleportationProperty
    {
        /// <inheritdoc />
        public event EventHandler<EventArgs> Teleported;

        /// <inheritdoc />
        public bool WasUsedToTeleport => wasUsedToTeleport;

        protected TeleportationAnchor TeleportationInteractable
        {
            get
            {
                if (interactable == null)
                {
                    interactable = GetComponent<TeleportationAnchor>();
                }

                return interactable;
            }
        }
        private TeleportationAnchor interactable;

        private Renderer[] renderers;
        private bool wasUsedToTeleport;

        protected void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            switch (TeleportationInteractable.teleportTrigger)
            {
                case BaseTeleportationInteractable.TeleportTrigger.OnActivated:
                    TeleportationInteractable.activated.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
                case BaseTeleportationInteractable.TeleportTrigger.OnDeactivated:
                    TeleportationInteractable.deactivated.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
                case BaseTeleportationInteractable.TeleportTrigger.OnSelectEntered:
                    TeleportationInteractable.selectEntered.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
                case BaseTeleportationInteractable.TeleportTrigger.OnSelectExited:
                    TeleportationInteractable.selectExited.AddListener(args =>
                    {
                        EmitTeleported();
                    });
                    break;
            }
        }
        
        /// <inheritdoc />
        public void Initialize()
        {
            wasUsedToTeleport = false;
        }

        /// <inheritdoc />
        public void FastForwardTeleport()
        {
            TeleportRequest teleportRequest = new TeleportRequest
            {
                requestTime = Time.time,
                matchOrientation = TeleportationInteractable.matchOrientation,
                destinationPosition = TeleportationInteractable.teleportAnchorTransform.position,
                destinationRotation = TeleportationInteractable.teleportAnchorTransform.rotation
            };

            TeleportationInteractable.teleportationProvider.QueueTeleportRequest(teleportRequest);
        }

        /// <inheritdoc />
        protected override void InternalSetLocked(bool lockState)
        {
            foreach (Collider collider in TeleportationInteractable.colliders)
            {
                collider.enabled = !lockState;
            }
            
            TeleportationInteractable.enabled = !lockState;

            if (renderers != null)
            {
                foreach (Renderer anchorRenderer in renderers)
                {
                    anchorRenderer.enabled = !lockState;
                }
            }
        }
        
        protected void EmitTeleported()
        {
            wasUsedToTeleport = true;
            Teleported?.Invoke(this, EventArgs.Empty);
        }
    }
}