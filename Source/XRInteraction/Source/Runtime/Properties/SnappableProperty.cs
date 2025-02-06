using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRBuilder.BasicInteraction.Properties;
using VRBuilder.Core.Properties;
using VRBuilder.XRInteraction.Interactables;

namespace VRBuilder.XRInteraction.Properties
{
    /// <summary>
    /// XR implementation of <see cref="ISnappableProperty"/>.
    /// </summary>
    [RequireComponent(typeof(GrabbableProperty))]
    public class SnappableProperty : LockableProperty, ISnappableProperty
    {
        [Header("Events")]
        [SerializeField]
        private UnityEvent<SnappablePropertyEventArgs> attachedToSnapZone = new UnityEvent<SnappablePropertyEventArgs>();

        [SerializeField]
        private UnityEvent<SnappablePropertyEventArgs> detachedFromSnapZone = new UnityEvent<SnappablePropertyEventArgs>();

        /// <inheritdoc />
        public UnityEvent<SnappablePropertyEventArgs> AttachedToSnapZone => attachedToSnapZone;

        /// <inheritdoc />
        public UnityEvent<SnappablePropertyEventArgs> DetachedFromSnapZone => detachedFromSnapZone;

        /// <summary>
        /// Returns true if the snappable object is snapped.
        /// </summary>
        public bool IsSnapped => SnappedZone != null;

        /// <summary>
        /// Will return the <see cref="SnapZoneProperty"/> of the <see cref="SnapZone"/> which snapped this object.
        /// </summary>
        public ISnapZoneProperty SnappedZone { get; set; }

        [SerializeField]
        [Tooltip("Will object be locked when it has been snapped.")]
        private bool lockObjectOnSnap;

        /// <inheritdoc />
        public bool LockObjectOnSnap
        {
            get => lockObjectOnSnap;
            set => lockObjectOnSnap = value;
        }

        /// <summary>
        /// Reference to attached <see cref="InteractableObject"/>.
        /// </summary>
        public XRBaseInteractable Interactable
        {
            get
            {
                if (interactable == null)
                {
                    interactable = GetComponent<InteractableObject>();
                }

                return interactable;
            }
        }

        private XRBaseInteractable interactable;

        protected new virtual void OnEnable()
        {
            base.OnEnable();

            Interactable.selectEntered.AddListener(HandleSnappedToDropZone);
            Interactable.selectExited.AddListener(HandleUnsnappedFromDropZone);

            InternalSetLocked(IsLocked);
        }

        protected new virtual void OnDisable()
        {
            Interactable.selectEntered.RemoveListener(HandleSnappedToDropZone);
            Interactable.selectExited.RemoveListener(HandleUnsnappedFromDropZone);
        }

        private void HandleSnappedToDropZone(SelectEnterEventArgs arguments)
        {
            IXRSelectInteractor interactor = arguments.interactorObject;
            SnappedZone = interactor.transform.GetComponent<SnapZoneProperty>();

            if (SnappedZone == null)
            {
                // Selector is not a snap zone.
                return;
            }

            if (LockObjectOnSnap)
            {
                SceneObject.RequestLocked(true);
            }

            EmitSnapped(SnappedZone);
        }

        private void HandleUnsnappedFromDropZone(SelectExitEventArgs arguments)
        {
            IXRSelectInteractor interactor = arguments.interactorObject;
            ISnapZoneProperty snapZone = interactor.transform.GetComponent<SnapZoneProperty>();

            if (snapZone == null)
            {
                // Selector is not a snap zone.
                return;
            }

            SnappedZone = null;
            EmitUnsnapped(snapZone);
        }

        /// <inheritdoc />
        protected override void InternalSetLocked(bool lockState)
        {
        }

        /// <summary>
        /// Invokes the <see cref="EmitSnapped"/> event.
        /// </summary>
        protected void EmitSnapped(ISnapZoneProperty snapZone)
        {
            AttachedToSnapZone?.Invoke(new SnappablePropertyEventArgs(this, snapZone));
        }

        /// <summary>
        /// Invokes the <see cref="EmitUnsnapped"/> event.
        /// </summary>
        protected void EmitUnsnapped(ISnapZoneProperty snapZone)
        {
            DetachedFromSnapZone?.Invoke(new SnappablePropertyEventArgs(this, snapZone));
        }

        /// <summary>
        /// Instantaneously snap the object into <paramref name="snapZone"/>
        /// </summary>
        public void FastForwardSnapInto(ISnapZoneProperty snapZone)
        {
            if (snapZone != null && snapZone is SnapZoneProperty snapDropZone)
            {
                snapDropZone.SnapZone.ForceSnap(this);
            }
        }
    }
}
