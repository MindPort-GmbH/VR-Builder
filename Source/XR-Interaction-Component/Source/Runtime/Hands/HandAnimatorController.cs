using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRBuilder.XRInteraction.Animation
{
    [RequireComponent(typeof(Animator))]
    public class HandAnimatorController : MonoBehaviour
    {
        private const string grabParameter = "Hand";
        private const string useParameter = "Index";
        private const string pointParameter = "Point";
        private const string teleportParameter = "Teleport";

        private Animator animator;
        private ActionBasedController baseController;
        private ActionBasedController teleportController;
        private ActionBasedController uiController;
        private ActionBasedControllerManager controllerManager;
        private bool isInitialized = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            controllerManager = GetComponentInParent<ActionBasedControllerManager>();
            baseController = controllerManager.BaseController.GetComponent<ActionBasedController>();
            teleportController = controllerManager.TeleportController.GetComponent<ActionBasedController>();
            uiController = controllerManager.UIController.GetComponent<ActionBasedController>();
            if (baseController != null && teleportController != null && uiController != null)
            {
                isInitialized = true;
            }
        }

        private void Update()
        {
            if (isInitialized == false)
            {
                return;
            }

            if(controllerManager.UIState.Enabled)
            {
                animator.SetBool(pointParameter, true);
            }
            else
            {
                animator.SetBool(pointParameter, false);
            }

            if(controllerManager.TeleportState.Enabled)
            {
                animator.SetBool(teleportParameter, true);
            }
            else
            {
                animator.SetBool(teleportParameter, false);
            }

            animator.SetFloat(grabParameter, baseController.selectActionValue.action.ReadValue<float>());
            animator.SetFloat(useParameter, baseController.activateActionValue.action.ReadValue<float>());
        }
    }
}