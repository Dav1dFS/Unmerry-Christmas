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

    [Header("Animation References")]
    [SerializeField] private Animator _animator; 

    // ── Input gathering ───────────────────────────────────────────────────────

    private void GatherInput()
    {
        if (PlayerFreezeManager.Instance?.isFrozen == true)
        {
            _input = Vector3.zero;
            return;
        }

        // Dynamic State Selection: Direct evaluation bypasses fragile Event Callbacks.
        bool leftPressed  = _moveLeft.IsPressed();
        bool rightPressed = _moveRight.IsPressed();
        bool upPressed    = _moveUp.IsPressed();
        bool downPressed  = _moveDown.IsPressed();

        // Horizontal Evaluation
        if (leftPressed && rightPressed)
        {
            if (_lockedH == 0) _lockedH = -1; // Fallback default
        }
        else if (leftPressed)   _lockedH = -1;
        else if (rightPressed)  _lockedH = 1;
        else                    _lockedH = 0;

        // Vertical Evaluation
        if (upPressed && downPressed)
        {
            if (_lockedV == 0) _lockedV = 1; // Fallback default
        }
        else if (upPressed)    _lockedV = 1;
        else if (downPressed)  _lockedV = -1;
        else                   _lockedV = 0;

        _input = new Vector3(_lockedH, 0, _lockedV);
    }

    // ── Speed & Animations ────────────────────────────────────────────────────

    private void UpdateSpeed()
    {
        if (isRolling) return;

        float targetSpeed = _input == Vector3.zero ? 0f
            : (_sprint.IsPressed() && _pushedObject == null) ? _sprintSpeed : _walkSpeed;
        float rate = _input != Vector3.zero ? _acceleration : _deceleration;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.deltaTime);

    }

    // ── Dynamic Camera Conversion Helper ──────────────────────────────────────

    /// <summary>
    /// Projects raw 2D horizontal/vertical inputs onto the ground plane 
    /// relative to the current Main Camera perspective.
    /// </summary>
    private Vector3 GetCameraRelativeDirection(Vector3 rawInput)
    {
        if (rawInput == Vector3.zero) return Vector3.zero;

        Camera mainCam = Camera.main;
        if (mainCam == null) return rawInput.normalized;

        // Extract camera alignment planes
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;

        // Flatten vectors completely onto the horizontal floor plane
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Combine inputs dynamically based on current screen perspective
        return (camForward * rawInput.z) + (camRight * rawInput.x);
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
            // Swapped out old .ToIso() static bias for our smart camera perspective tracking!
            Vector3 relativeDir = GetCameraRelativeDirection(_input);
            Quaternion targetRot = Quaternion.LookRotation(relativeDir, Vector3.up);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }
    }

    // ── Movement (FixedUpdate) ────────────────────────────────────────────────

    private void Move()
    {
        if (PlayerFreezeManager.Instance?.isFrozen == true)
        {
                AudioManager.instance?.StopWalkingSound();
                AudioManager.instance?.StopPushingSound();
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
            AudioManager.instance?.StopWalkingSound();
            AudioManager.instance?.StopPushingSound();
            _rb.linearVelocity = new Vector3(
                rollDirection.x * _rollForce,
                yVel,
                rollDirection.z * _rollForce);
        }
        else if (_pushedObject != null)
        {
            AudioManager.instance?.StopWalkingSound();
            if (_input != Vector3.zero)
            {
                AudioManager.instance?.PlayPushingSound();
                // Replaced hardcoded .ToIso() with modern camera tracking matrix while pushing objects
                Vector3 currentPushDir = GetCameraRelativeDirection(_input).normalized;
                Vector3 contactWorld = _pushedObject.transform.TransformPoint(_contactLocalPos);
                _pushedObject.ApplyPushForce(currentPushDir, contactWorld);
            }
            else
            {
                AudioManager.instance?.StopPushingSound();
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
            AudioManager.instance?.StopPushingSound();
            if (_input != Vector3.zero && isGrounded)
            {
                AudioManager.instance?.PlayWalkingSound();
            }
            else
            {
                AudioManager.instance?.StopWalkingSound();
            }
            // Calculate velocity relative to our forward look vector, which now shifts beautifully
            // alongside the dynamic camera tracking rules inside Look()
            Vector3 moveDir    = transform.forward * (_input != Vector3.zero ? _currentSpeed : 0f);
            _rb.linearVelocity = new Vector3(moveDir.x, yVel, moveDir.z);
        }

        // Discard any jump press queued while the player was airborne.
        if (!isGrounded) jump = false;

        if (jump && isGrounded && _pushedObject == null)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump       = false;   
            isGrounded = false;
            AudioManager.instance?.PlayOneShot(jumpSound, transform.position);
        }
    }
}