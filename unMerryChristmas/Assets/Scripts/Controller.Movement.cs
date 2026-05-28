using UnityEngine;

public partial class Controller
{
    // ── Settings ─────────────────────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float _walkSpeed     = 5f;
    [SerializeField] private float _sprintSpeed   = 10f;
    [SerializeField] private float _acceleration  = 10f;
    [SerializeField] private float _deceleration  = 15f;
    [SerializeField] private float _rotationSpeed = 10f;

    // ── Input gathering ───────────────────────────────────────────────────────

    private void GatherInput()
    {
        if (PlayerFreezeManager.Instance?.isFrozen == true)
        {
            _input = Vector3.zero;
            return;
        }
        _input = new Vector3(_lockedH, 0, _lockedV);
    }

    // ── Speed ─────────────────────────────────────────────────────────────────

    private void UpdateSpeed()
    {
        if (isRolling) return;

        float targetSpeed = _input == Vector3.zero ? 0f
            : (_sprint.IsPressed() && _pushedObject == null) ? _sprintSpeed : _walkSpeed;
        float rate = _input != Vector3.zero ? _acceleration : _deceleration;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.deltaTime);
    }

    // ── Rotation ──────────────────────────────────────────────────────────────

    private void Look()
    {
        if (_pushedObject != null)
        {
            // Face the push-contact point so the player always pushes front-on
            Vector3 contactWorld = _pushedObject.transform.TransformPoint(_contactLocalPos);
            Vector3 toContact    = new Vector3(
                contactWorld.x - transform.position.x, 0f,
                contactWorld.z - transform.position.z);
            if (toContact.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(toContact.normalized, Vector3.up),
                    _rotationSpeed * Time.deltaTime);
            return;
        }

        if (_input != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }
    }

    // ── Movement (FixedUpdate) ────────────────────────────────────────────────

    private void Move()
    {
        if (PlayerFreezeManager.Instance?.isFrozen == true)
        {
            _input = Vector3.zero;
            return;
        }

        // Prevent unwanted spin from physics interactions
        _rb.angularVelocity = Vector3.zero;

        if (isRolling)
        {
            _rb.MovePosition(transform.position + rollDirection * _rollForce * Time.fixedDeltaTime);
        }
        else if (_pushedObject != null)
        {
            if (_input != Vector3.zero)
            {
                Vector3 contactWorld = _pushedObject.transform.TransformPoint(_contactLocalPos);
                _pushedObject.ApplyPushForce(_input.ToIso().normalized, contactWorld);
            }
            Vector3 targetWorld = _pushedObject.transform.TransformPoint(_playerLocalPos);
            _rb.MovePosition(new Vector3(targetWorld.x, transform.position.y, targetWorld.z));
        }
        else
        {
            _rb.MovePosition(transform.position
                + transform.forward
                * (_input != Vector3.zero ? 1f : 0f)
                * _currentSpeed
                * Time.fixedDeltaTime);
        }

        // Jump is consumed here so physics force is applied in FixedUpdate.
        // IMPORTANT: clear flags BEFORE the audio call so a missing AudioManager
        // can never prevent the state from advancing (infinite-jump safeguard).
        if (jump && isGrounded && _pushedObject == null)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump       = false;   // must be before audio — exception must not leave these set
            isGrounded = false;
            AudioManager.instance?.PlayOneShot(jumpSound, transform.position);
        }
    }
}
