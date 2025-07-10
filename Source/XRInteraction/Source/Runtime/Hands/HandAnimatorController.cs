using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRBuilder.XRInteraction.XRI.StarterAssets;

namespace VRBuilder.XRInteraction.Animation
{
    /// <summary>
    /// Reads values on the current controller Select and Activate actions and uses them to drive hand animations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HandAnimatorController: MonoBehaviour
    {
        [Header("Animator Parameters")]
        [SerializeField]
        [Tooltip("Float parameter corresponding to select value.")]
        private string selectFloat = "Select";

        [SerializeField]
        [Tooltip("Float parameter corresponding to activate value.")]
        private string activateFloat = "Activate";

        [SerializeField]
        [Tooltip("Bool parameter true if UI state enabled.")]
        private string UIStateBool = "UIEnabled";

        [SerializeField]
        [Tooltip("Bool parameter true if teleport state enabled.")]
        private string teleportStateBool = "TeleportEnabled";

        [Header("Input Actions")]
        [SerializeField]
        [Tooltip("Input action reference for select value.")]
        private InputActionReference selectActionReference;

        [SerializeField]
        [Tooltip("Input action reference for activate value.")]
        private InputActionReference activateActionReference;

        [SerializeField]
        [Tooltip("Input action reference for UI mode.")]
        private InputActionReference uiModeActionReference;

        [SerializeField]
        [Tooltip("Input action reference for teleport mode.")]
        private InputActionReference teleportModeActionReference;
        
        [Header("Object References")]
        [SerializeField]
        [Tooltip("Interactors to check for selection. If any of these are selecting an object, UI mode will not be enabled.")]
        private List<XRBaseInteractor> interactors;
        
        [SerializeField]
        [Tooltip("Controller input action manager.")]
        private ControllerInputActionManager controllerManager;
        
        private Animator animator;

        /// <summary>
        /// True if the controller is in UI mode.
        /// </summary>
        public bool IsUIMode { get; private set; }

        /// <summary>
        /// True if the controller is in teleport mode.
        /// </summary>
        public bool IsTeleportMode { get; private set; }

        /// <summary>
        /// Current controller select value.
        /// </summary>
        public float SelectValue { get; private set; }

        /// <summary>
        /// Current controller activate value.
        /// </summary>
        public float ActivateValue { get; private set; }

        private void Start()
        {
            animator = GetComponent<Animator>();

            controllerManager ??= GetComponentInParent<ControllerInputActionManager>();
            
            if(controllerManager == null)
            {
                Debug.LogWarning($"{nameof(HandAnimatorController)} could not find a {nameof(ControllerInputActionManager)} on {gameObject.name}.");
            }

            if (selectActionReference == null || activateActionReference == null)
            {
                Debug.LogWarning($"{nameof(HandAnimatorController)} could not retrieve the necessary input actions. {gameObject.name} will not animate properly.");
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }
            
            IsUIMode = IsActionActive(uiModeActionReference) || (interactors != null && interactors.Any(interactor => interactor != null && interactor.hasHover));
            
            IsTeleportMode = IsActionActive(teleportModeActionReference);
            
            if (selectActionReference != null && selectActionReference.action != null)
            {
                SelectValue = selectActionReference.action.ReadValue<float>();
            }
            
            if (activateActionReference != null && activateActionReference.action != null)
            {
                ActivateValue = activateActionReference.action.ReadValue<float>();
            }
            
            animator.SetBool(UIStateBool, IsUIMode);
            animator.SetBool(teleportStateBool, IsTeleportMode);
            animator.SetFloat(selectFloat, SelectValue);
            animator.SetFloat(activateFloat, ActivateValue);
        }

        private static bool IsActionActive(InputActionReference actionReference)
        {
            if (actionReference != null && actionReference.action is { enabled: true })
            {
                switch (actionReference.action.expectedControlType)
                {
                    case "Button":
                        return actionReference.action.ReadValue<float>() > 0.5f;
                    case "Vector2":
                        return actionReference.action.ReadValue<Vector2>().magnitude > 0.5f;
                    case "Digital":
                        return actionReference.action.ReadValue<bool>();
                }
            }

            return false;
        }
    }
}