using UnityEngine;

public partial class Controller
{
    // Drives Animator parameters based on movement and action state

    private bool rollTriggered;
    private bool jumpTriggered;

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        bool isMoving = _input != Vector3.zero
                        && !PlayerFreezeManager.Instance.isFrozen;

        _animator.SetBool("Walk", isMoving && !isRolling);
        _animator.SetBool("IsGrounded", isGrounded);
        _animator.SetBool("Aim", isAiming);

        if (jump && isGrounded && !jumpTriggered)
        {
            _animator.SetTrigger("Jump");
            jumpTriggered = true;
        }

        if (isRolling && !rollTriggered)
        {
            _animator.SetTrigger("Roll");
            rollTriggered = true;
        }

        if (!isRolling)
        {
            rollTriggered = false;
        }
    }

    /// <summary>
    /// Plays the elf's one-shot "found the drawing book" introductory animation
    /// (Level Design Document §3.3). Fires the "PickupBook" animator trigger.
    /// Safe no-op if the Animator has no such trigger/state yet — SetTrigger
    /// logs a warning but never throws, so the rest of the pickup still runs.
    /// </summary>
    public void PlayBookPickupAnimation()
    {
        if (_animator == null) return;
        _animator.SetTrigger("PickupBook");
    }
}