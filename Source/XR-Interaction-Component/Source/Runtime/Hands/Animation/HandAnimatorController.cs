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
        private const string grabParameter = "Hand";
        private const string useParameter = "Index";
        private const string pointParameter = "Point";
        private const string teleportParameter = "Teleport";

        private Animator animator;
        private ActionBasedController baseController;
        private ActionBasedControllerManager controllerManager;

        private void Start()
        {
            animator = GetComponent<Animator>();
            controllerManager = GetComponentInParent<ActionBasedControllerManager>();

            if (controllerManager != null)
            {
                baseController = controllerManager.BaseController.GetComponent<ActionBasedController>();
            }

            if(baseController == null)
            {
                Debug.LogWarning($"{typeof(HandAnimatorController).Name} could not retrieve the matching {typeof(ActionBasedController).Name}. {gameObject.name} will not animate.");
            }
        }

        private void Update()
        {
            if (baseController == null)
            {
                return;
            }

            if (controllerManager.UIState.Enabled) 
            {
                animator.SetBool(pointParameter, true);
            }
            else
            {
                animator.SetBool(pointParameter, false);
            }

            if (controllerManager.TeleportState.Enabled) 
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