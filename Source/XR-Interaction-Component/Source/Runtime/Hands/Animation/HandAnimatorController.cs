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
            if (controller == null)
            {
                return;
            }

            if (controllerManager != null)
            {
                if (controllerManager.UIState.Enabled)
                {
                    animator.SetBool(UIStateBool, true);
                }
                else
                {
                    animator.SetBool(UIStateBool, false);
                }

                if (controllerManager.TeleportState.Enabled)
                {
                    animator.SetBool(teleportStateBool, true);
                }
                else
                {
                    animator.SetBool(teleportStateBool, false);
                }
            }

            animator.SetFloat(selectFloat, controller.selectActionValue.action.ReadValue<float>());
            animator.SetFloat(activateFloat, controller.activateActionValue.action.ReadValue<float>());
        }
    }
}