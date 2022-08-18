using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private const string grabParameter = "Grab";
    private const string pointParameter = "Point";

    private Animator animator;
    ActionBasedController controller;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(controller == null)
        {
            controller = GetComponentInParent<ActionBasedController>();
        }

        if (controller != null)
        {
            Debug.Log(controller.selectActionValue.action.ReadValue<float>());
            animator.SetFloat(grabParameter, controller.selectActionValue.action.ReadValue<float>());
            animator.SetFloat(pointParameter, controller.activateActionValue.action.ReadValue<float>());
        }
    }
}
