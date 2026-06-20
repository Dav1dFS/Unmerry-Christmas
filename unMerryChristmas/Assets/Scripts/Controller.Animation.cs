using UnityEngine;

public partial class Controller
{
    // Drives Animator parameters based on movement and action state

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        bool isMoving = _input != Vector3.zero
                        && !PlayerFreezeManager.Instance.isFrozen;

        _animator.SetBool("Walk", isMoving && !isRolling);
    }
}