using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRBuilder.XRInteraction;

[RequireComponent(typeof(Animator))]
public class HandAnimatorController : MonoBehaviour
{
    private const string grabParameter = "Grab";
    private const string pointParameter = "Point";

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
        if(baseController != null && teleportController != null && uiController != null)
        {
            isInitialized = true;
        }
    }

    private void Update()
    {
        if(isInitialized == false)
        {
            return;
        }
        
        if(controllerManager.SelectState.Enabled)
        {
            animator.SetFloat(grabParameter, baseController.selectActionValue.action.ReadValue<float>());
            animator.SetFloat(pointParameter, baseController.activateActionValue.action.ReadValue<float>());
        }
        else if(controllerManager.TeleportState.Enabled)
        {
            animator.SetFloat(grabParameter, baseController.selectActionValue.action.ReadValue<float>());
            animator.SetFloat(pointParameter, baseController.activateActionValue.action.ReadValue<float>());
        }
        else if (controllerManager.UIState.Enabled)
        {
            animator.SetFloat(grabParameter, Mathf.Clamp01(animator.GetFloat(grabParameter) - Time.deltaTime * 10));
            animator.SetFloat(pointParameter, baseController.selectActionValue.action.ReadValue<float>());
        }
    }
}
