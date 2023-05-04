using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.XRInteraction.Animation
{
    /// <summary>
    /// Reads values on current controller Select and Activate actions and uses them to drive hand animations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HandAnimatorController : MonoBehaviour
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

        private Animator animator;

        [Header("Object References")]
        [SerializeField]
        [Tooltip("Controller to read input actions from.")]
        private ActionBasedController controller;

        [SerializeField]
        [Tooltip("Controller manager needed to set state parameters.")]
        private ActionBasedControllerManager controllerManager;

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

            if (controllerManager == null)
            {
                controllerManager = GetComponentInParent<ActionBasedControllerManager>();
            }

            if (controllerManager != null && controller == null)
            {
                controller = controllerManager.BaseController.GetComponent<ActionBasedController>();
            }

            if(controller == null)
            {
                Debug.LogWarning($"{typeof(HandAnimatorController).Name} could not retrieve the matching {typeof(ActionBasedController).Name}. {gameObject.name} will not animate.");
            }
        }

        private void Update()
        {
            if (controller == null || controller.enableInputActions == false)
            {
                return;             
            }

            if (controllerManager != null)
            {
                if (controllerManager.UIState.Enabled)
                {
                    IsUIMode = true;
                }
                else
                {
                    IsUIMode = false;
                }

                if (controllerManager.TeleportState.Enabled)
                {
                    IsTeleportMode = true;
                }
                else
                {
                    IsTeleportMode = false;
                }
            }

            SelectValue = controller.selectActionValue.action.ReadValue<float>();
            ActivateValue = controller.activateActionValue.action.ReadValue<float>();

            animator.SetBool(UIStateBool, IsUIMode);
            animator.SetBool(teleportStateBool, IsTeleportMode);
            animator.SetFloat(selectFloat, SelectValue);
            animator.SetFloat(activateFloat, ActivateValue);
        }
    }
}