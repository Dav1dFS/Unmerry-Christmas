using UnityEngine;

public partial class Controller
{
    // Drives Animator parameters based on movement and action state

    private bool rollTriggered;

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        bool isMoving = _input != Vector3.zero
                        && !PlayerFreezeManager.Instance.isFrozen;

        _animator.SetBool("Walk", isMoving && !isRolling);
        _animator.SetBool("IsGrounded", isGrounded);
        _animator.SetBool("Aim", isAiming);

        if (jump && isGrounded)
            _animator.SetTrigger("Jump");

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
}