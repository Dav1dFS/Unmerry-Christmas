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

        // Sanitize stale locks — if the key that owns the lock is no longer
        // physically pressed (e.g. after focus loss or a missed canceled event)
        // clear it and restore the opposite direction if that key is still held.
        if (_lockedH == -1 && !_moveLeft.IsPressed())
            _lockedH = _moveRight.IsPressed() ? 1 : 0;
        else if (_lockedH == 1 && !_moveRight.IsPressed())
            _lockedH = _moveLeft.IsPressed() ? -1 : 0;

        if (_lockedV == 1 && !_moveUp.IsPressed())
            _lockedV = _moveDown.IsPressed() ? -1 : 0;
        else if (_lockedV == -1 && !_moveDown.IsPressed())
            _lockedV = _moveUp.IsPressed() ? 1 : 0;

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
            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            return;
        }

        // Prevent unwanted spin from physics interactions
        _rb.angularVelocity = Vector3.zero;

        // When grounded, clamp preserved Y to ≤ 0: depenetration or contact forces
        // can write a positive Y into linearVelocity, and reading it back each step
        // creates a feedback loop that flings the player upward ("intensification").
        float yVel = isGrounded ? Mathf.Min(_rb.linearVelocity.y, 0f) : _rb.linearVelocity.y;

        if (isRolling)
        {
            _rb.linearVelocity = new Vector3(
                rollDirection.x * _rollForce,
                yVel,
                rollDirection.z * _rollForce);
        }
        else if (_pushedObject != null)
        {
            if (_input != Vector3.zero)
            {
                Vector3 contactWorld = _pushedObject.transform.TransformPoint(_contactLocalPos);
                _pushedObject.ApplyPushForce(_input.ToIso().normalized, contactWorld);
            }
            // Target = box centre + fixed world-space XZ offset captured at grab time.
            // Using world-space (not local) means box Y-rotation never orbits the player.
            Vector3 boxPos    = _pushedObject.transform.position;
            Vector3 targetPos = new Vector3(
                boxPos.x + _playerWorldOffset.x,
                transform.position.y,
                boxPos.z + _playerWorldOffset.z);
            _rb.linearVelocity = new Vector3(
                (targetPos.x - transform.position.x) / Time.fixedDeltaTime,
                yVel,
                (targetPos.z - transform.position.z) / Time.fixedDeltaTime);
        }
        else
        {
            Vector3 moveDir    = transform.forward * (_input != Vector3.zero ? _currentSpeed : 0f);
            _rb.linearVelocity = new Vector3(moveDir.x, yVel, moveDir.z);
        }

        // Discard any jump press queued while the player was airborne.
        // Without this, a button press at the apex (velocity ≈ 0, still in the
        // air) would wait for the first moment isGrounded becomes true and then
        // fire — producing a mid-descent "double jump".
        if (!isGrounded) jump = false;

        if (jump && isGrounded && _pushedObject == null)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump       = false;   // must be before audio — exception must not leave these set
            isGrounded = false;
            AudioManager.instance?.PlayOneShot(jumpSound, transform.position);
        }
    }
}
