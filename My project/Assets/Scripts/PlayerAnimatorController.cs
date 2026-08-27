using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetState(float speedInput, bool grounded, float verticalSpeed, bool pushing)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", speedInput);
        animator.SetBool("Grounded", grounded);
        animator.SetFloat("VerticalSpeed", verticalSpeed);
        animator.SetBool("Pushing", pushing);
    }

    public void PlayDeath()
    {
        if (animator == null) return;
        animator.SetTrigger("Death");
    }
}
