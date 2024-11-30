using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();
    }

    // Method to trigger the 'idle' animation
    public void TriggerIdle()
    {
        animator.SetTrigger("trigger-ph-idle");
    }

    // Method to trigger the 'nodding' animation
    public void TriggerNodding()
    {
        animator.SetTrigger("trigger-ph-nodding");
    }
}
