using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 10f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 15f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float _rollForce = 8f;
    [SerializeField] private float _rollDuration = 0.4f;
    [SerializeField] private float _rollCooldown = 1.5f;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    [SerializeField] private LayerMask trajectoryCollisionMask;

    [SerializeField] private InputAction _moveLeft;
    [SerializeField] private InputAction _moveRight;
    [SerializeField] private InputAction _moveUp;
    [SerializeField] private InputAction _moveDown;
    [SerializeField] private InputAction _sprint;
    [SerializeField] private InputAction _jump;
    [SerializeField] private InputAction _interact;
    [SerializeField] private InputAction _roll;
    [SerializeField] private InputAction _spawnGift;
    [SerializeField] private InputAction _dropGift;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private LayerMask pushableLayer;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private GameObject _giftBombPrefab;
    

    private Vector3 _input;
    private float _currentSpeed;
    private int _lockedH = 0;
    private int _lockedV = 0;
    private float jumpForce = 8f;
    private bool jump = false;
    private bool isGrounded = true;
    private bool isAiming = false;
    private float holdTime = 0f;

    private bool isRolling = false;
    private float rollTimer = 0f;
    private float rollCooldownTimer = 0f;
    private Vector3 rollDirection;

    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private float pushableRange = 0.8f;

    [SerializeField] private float aimHoldThreshold = 0.2f;
    [SerializeField] private float maxThrowChargeTime = 2f;

    private bool isHoldingInteract = false;
    private bool hasEnteredAimMode = false;


    private PickupObject heldObject;
    private PushableObject _pushedObject;
    private ExplosivePresent _giftInHand;
    private Vector3 _contactLocalPos; // contact point on the object's face, in object local space
    private Vector3 _playerLocalPos;  // player position in object local space at grab time

    [SerializeField] private EventReference jumpSound;
    [SerializeField] private EventReference landSound;
    [SerializeField] private EventReference grabSound;
    [SerializeField] private EventReference throwSound;
    [SerializeField] private EventReference chargeThrowSound;
    private FMOD.Studio.EventInstance chargeThrowInstance;
    private bool _wasGrounded = true;

    private void OnEnable()
    {
        _moveLeft.Enable();
        _moveRight.Enable();
        _moveUp.Enable();
        _moveDown.Enable();
        _sprint.Enable();
        _jump.Enable();
        _interact.Enable();
        _roll.Enable();
        _spawnGift.Enable();
        _dropGift.Enable();

        _jump.started += _ => jump = true;
        _interact.started += _ => StartInteractHold();
        _interact.canceled += _ => ReleaseInteractHold();
        _roll.started += _ => TryStartRoll();
        _spawnGift.started += _ => TrySpawnGift();
        _dropGift.started += _ => DropGift();

        _moveLeft.started  += _ => { if (_lockedH == 0) _lockedH = -1; };
        _moveLeft.canceled += _ => { if (_lockedH == -1) _lockedH = _moveRight.IsPressed() ? 1 : 0; };

        _moveRight.started  += _ => { if (_lockedH == 0) _lockedH = 1; };
        _moveRight.canceled += _ => { if (_lockedH == 1) _lockedH = _moveLeft.IsPressed() ? -1 : 0; };

        _moveUp.started  += _ => { if (_lockedV == 0) _lockedV = 1; };
        _moveUp.canceled += _ => { if (_lockedV == 1) _lockedV = _moveDown.IsPressed() ? -1 : 0; };

        _moveDown.started  += _ => { if (_lockedV == 0) _lockedV = -1; };
        _moveDown.canceled += _ => { if (_lockedV == -1) _lockedV = _moveUp.IsPressed() ? 1 : 0; };
    }

    private void OnDisable()
    {
        _moveLeft.Disable();
        _moveRight.Disable();
        _moveUp.Disable();
        _moveDown.Disable();
        _sprint.Disable();
        _jump.Disable();
        _interact.Disable();
        _roll.Disable();
        _spawnGift.Disable();
        _dropGift.Disable();
    }
    // locked abilities functions
    void TryStartRoll()
    {
        if (_pushedObject != null) return;

        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Rolling))
        {
            Debug.Log("Rolling ability is not unlocked yet!");
            return;
        }
        StartRoll();
    }

    void StartInteractHold()
{
    if (_pushedObject != null) return;

    isHoldingInteract = true;
    hasEnteredAimMode = false;
    holdTime = 0f;
}

void ReleaseInteractHold()
{
    if (!isHoldingInteract) return;

    isHoldingInteract = false;

    
    if (hasEnteredAimMode)
    {
        if (heldObject != null)
        {
            throwObject();
        }

        isAiming = false;
        holdTime = 0f;
        StopChargeSound();
    }
   
    else
    {
        checkHands();
    }
}

    void TrySpawnGift()
    {
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.ExplosingPresents))
        {
            Debug.Log("Exploding Presents ability is not unlocked yet!");
            return;
        }
        if (heldObject != null || _giftInHand != null) return;

        GameObject obj = Instantiate(_giftBombPrefab, holdPoint.position, Quaternion.identity);
        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        _giftInHand = obj.GetComponent<ExplosivePresent>();

        Collider giftCol = obj.GetComponent<Collider>();
        if (giftCol != null) giftCol.enabled = false;
    }

    void ThrowGift()
    {
        if (_giftInHand == null) return;

        ExplosivePresent gift = _giftInHand;
        _giftInHand = null;

        gift.transform.SetParent(null);
        gift.Arm();

        Collider giftCol = gift.GetComponent<Collider>();
        if (giftCol != null) giftCol.enabled = true;

        Rigidbody giftRb = gift.GetComponent<Rigidbody>();
        if (giftRb != null)
        {
            giftRb.isKinematic = false;
            float force = Mathf.Clamp(holdTime * throwForce, 5f, 15f);
            Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
            giftRb.AddForce(throwDirection.normalized * force, ForceMode.Impulse);
            AudioManager.instance.PlayOneShot(throwSound, this.transform.position);
        }

        isAiming = false;
        holdTime = 0f;
        StopChargeSound();
    }

    void DropGift()
    {
        if (_giftInHand == null) return;

        ExplosivePresent gift = _giftInHand;
        _giftInHand = null;

        gift.transform.SetParent(null);
        gift.Arm();

        Collider giftCol = gift.GetComponent<Collider>();
        if (giftCol != null) giftCol.enabled = true;

        Rigidbody giftRb = gift.GetComponent<Rigidbody>();
        if (giftRb != null)
            giftRb.isKinematic = false;
    }

    private void StopChargeSound()
    {
        if (chargeThrowInstance.isValid())
        {
            chargeThrowInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            chargeThrowInstance.release();
            chargeThrowInstance.clearHandle();
        }
    }

    void TryStartAiming()
    {
        if (_pushedObject != null) return;
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing))
        {
            Debug.Log("Throwing ability is not unlocked yet!");
            return;
        }
        StartAiming();
    }

    void TryStartThrowing()
    {
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing)) return;

        if (_giftInHand != null)
        {
            ThrowGift();
        }
        else
        {
            throwObject();
        }          
    }

    void StartAiming()
    {
        if (heldObject != null || _giftInHand != null)
        {
            isAiming = true;
            holdTime = 0f;
            chargeThrowInstance = RuntimeManager.CreateInstance(chargeThrowSound);
            RuntimeManager.AttachInstanceToGameObject(chargeThrowInstance, transform, _rb);
            chargeThrowInstance.start();
        }
    }

    void throwObject() 
    {
        if (heldObject != null)
        {
            isAiming = false;
            float normalizedCharge =Mathf.Clamp01((holdTime - aimHoldThreshold) / maxThrowChargeTime);

            float force =Mathf.Lerp(5f, 15f, normalizedCharge);

            Rigidbody objectRb = heldObject.GetComponent<Rigidbody>();
            if (objectRb != null)
            {
                ThrowableImpact impact = heldObject.GetComponent<ThrowableImpact>();
                if (impact != null) impact.SetThrown();

                DropObject();
                Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
                objectRb.AddForce(throwDirection.normalized * force, ForceMode.Impulse);
                AudioManager.instance.PlayOneShot(throwSound, this.transform.position);
            }
        }
    }

    void StartRoll()
    {
        if (isRolling || rollCooldownTimer > 0f) return;

        rollDirection = transform.forward;
        isRolling = true;
        rollTimer = _rollDuration;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    void checkHands()
    {
        if (heldObject != null)
        {
            DropObject();
        }
        else if (_pushedObject != null)
        {
            ReleasePushable();
        }
        else if (_giftInHand != null)
        {
            DropGift();
        }
        else
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        Collider[] pickupHits   = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);
        Collider[] pushableHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);
        Collider[] hits = new Collider[pickupHits.Length + pushableHits.Length];
        pickupHits.CopyTo(hits, 0);
        pushableHits.CopyTo(hits, pickupHits.Length);

        float closestDistance = Mathf.Infinity;
        PickupObject closestObject = null;
        Collider closestCollectable = null;
        PushableObject closestPushable = null;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance >= closestDistance) continue;

            if (hit.CompareTag("Collectable"))
            {
                closestDistance = distance;
                closestCollectable = hit;
                closestObject = null;
                closestPushable = null;
            }
            else if (hit.CompareTag("Token"))
            {
                closestDistance = distance;
                closestCollectable = hit;
                closestObject = null;
                closestPushable = null;
            }
            else
            {
                PickupObject pickup = hit.GetComponent<PickupObject>();
                if (pickup != null)
                {
                    closestDistance = distance;
                    closestObject = pickup;
                    closestPushable = null;
                }
                else
                {
                    PushableObject pushable = hit.GetComponent<PushableObject>();
                    if (pushable != null)
                    {
                        closestDistance = distance;
                        closestPushable = pushable;
                    }
                }
            }
        }

        if (closestCollectable != null)
        {
            if (closestCollectable.CompareTag("Token"))
            {
                AbilityToken token = closestCollectable.GetComponent<AbilityToken>();
                if (token != null)
                    AbilityTokenManager.Instance.Unlock(token.Ability);
            }
            else
            {
                CollectableManager.Instance.Collect();
            }
            Destroy(closestCollectable.gameObject);
        }
        else if (closestObject != null)
        {
            heldObject = closestObject;
            heldObject.OnPickup(holdPoint);
            AudioManager.instance.PlayOneShot(grabSound, this.transform.position);
        }
        else if (closestPushable != null)
        {
            if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Pushing))
            {
                Debug.Log("Pushing ability is not unlocked yet!");
                return;
            }

            // Raycast from player toward the object to find the exact face contact point
            Vector3 dirToObj = (closestPushable.transform.position - transform.position).normalized;
            Vector3 contactWorldPos;
            if (Physics.Raycast(transform.position, dirToObj, out RaycastHit contactHit, pickupRange * 2f, pushableLayer))
                contactWorldPos = contactHit.point;
            else
                contactWorldPos = closestPushable.GetComponent<Collider>().ClosestPoint(transform.position);

            _pushedObject = closestPushable;
            _contactLocalPos = closestPushable.transform.InverseTransformPoint(contactWorldPos);
            _playerLocalPos  = closestPushable.transform.InverseTransformPoint(transform.position);
            _pushedObject.OnGrab();
        }
    }

    void ReleasePushable()
    {
        _pushedObject.OnRelease();
        _pushedObject = null;
    }

    void ClearAbilityInputs()
    {
        jump = false;
        isAiming = false;
        holdTime = 0f;
        StopChargeSound();
    }

    void DropObject()
    {
        heldObject.OnDrop();
        heldObject = null;
    }

    void Update()
    {
        bool currentlyGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f);

        if (currentlyGrounded && !_wasGrounded)
        {
            float surfaceValue = 0f; // Default surface

            if (hit.collider.CompareTag("Wood"))
            {
                surfaceValue = 0f;
            }
            else if (hit.collider.CompareTag("Stone"))
            {
                surfaceValue = 1f;
            }
            else if (hit.collider.CompareTag("Metal"))
            {
                surfaceValue = 2f;
            }
            else if (hit.collider.CompareTag("Snow"))
            {
                surfaceValue = 3f;
            }

            AudioManager.instance.PlayOneShotWithParameter(landSound, transform.position, "SurfaceType", surfaceValue);
        }

        isGrounded = currentlyGrounded;
        _wasGrounded = isGrounded;

        if (rollCooldownTimer > 0f)
        {
            rollCooldownTimer -= Time.deltaTime;
        }
        if (isRolling)
        { 
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
            {
                isRolling = false;
                rollCooldownTimer = _rollCooldown;
            }
        }
        if (isHoldingInteract && heldObject != null)
        {
            holdTime += Time.deltaTime;

            if (!hasEnteredAimMode && holdTime >= aimHoldThreshold)
            {
                if (AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing))
                {
                    hasEnteredAimMode = true;
                    isAiming = true;
                    chargeThrowInstance = RuntimeManager.CreateInstance(chargeThrowSound);
                    RuntimeManager.AttachInstanceToGameObject(chargeThrowInstance, transform, _rb);
                    chargeThrowInstance.start();
                }
            }
        }

        if (isAiming)
        {
            DrawTrajectory();
        }
        else
        {
            trajectoryLine.enabled = false;
        }
        if (_pushedObject != null)
        {
            if (!_pushedObject.IsWithinPushDistance())
                ReleasePushable();
            else
                ClearAbilityInputs();
        }

        GatherInput();
        UpdateSpeed();
        Look();
    }

    void FixedUpdate()
    {
        Move();
    }

    void DrawTrajectory()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;

        float normalizedCharge =Mathf.Clamp01((holdTime - aimHoldThreshold) / maxThrowChargeTime);

        float force =Mathf.Lerp(5f, 15f, normalizedCharge);

        Vector3 startPosition = holdPoint.position;

        Vector3 throwDirection =
            (transform.forward + Vector3.up * 0.5f).normalized;

        Vector3 startVelocity = throwDirection * force;

        Vector3 previousPoint = startPosition;

        trajectoryLine.positionCount = trajectoryPoints;

        int pointCount = 0;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;

            Vector3 point =
                startPosition +
                startVelocity * t +
                0.5f * Physics.gravity * t * t;

            if (Physics.Linecast(previousPoint, point, out RaycastHit hit, trajectoryCollisionMask))
            {
                trajectoryLine.positionCount = pointCount + 1;
                trajectoryLine.SetPosition(pointCount, hit.point);
                break;
            }

            trajectoryLine.SetPosition(pointCount, point);

            previousPoint = point;
            pointCount++;
        }
    }


    void GatherInput()
    {
        if (PlayerFreezeManager.Instance.isFrozen)
        {
            _input = Vector3.zero;
            return;
        }
        _input = new Vector3(_lockedH, 0, _lockedV);
    }

    void UpdateSpeed()
    {
        if (isRolling) return;

        float targetSpeed = _input == Vector3.zero ? 0f :
            (_sprint.IsPressed() && _pushedObject == null) ? _sprintSpeed : _walkSpeed;
        float rate = _input != Vector3.zero ? _acceleration : _deceleration;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.deltaTime);
    }

    void Look()
    {
        if (_pushedObject != null)
        {
            Vector3 contactWorld = _pushedObject.transform.TransformPoint(_contactLocalPos);
            Vector3 toContact = new Vector3(contactWorld.x - transform.position.x, 0f, contactWorld.z - transform.position.z);
            if (toContact.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(toContact.normalized, Vector3.up), _rotationSpeed * Time.deltaTime);
            return;
        }
        if (_input != Vector3.zero)
        {
            var targetRot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        if (PlayerFreezeManager.Instance.isFrozen)
        {
            _input = Vector3.zero;
            return;
        }

        if (isRolling)
        {
            _rb.MovePosition(transform.position + rollDirection * _rollForce * Time.fixedDeltaTime);
        }
        else if (_pushedObject != null)
        {
            if (_input != Vector3.zero)
            {
                Vector3 contactWorld = _pushedObject.transform.TransformPoint(_contactLocalPos);
                Vector3 inputDir = _input.ToIso().normalized;
                _pushedObject.ApplyPushForce(inputDir, contactWorld);
            }

            Vector3 targetWorld = _pushedObject.transform.TransformPoint(_playerLocalPos);
            _rb.MovePosition(new Vector3(targetWorld.x, transform.position.y, targetWorld.z));
        }
        else
        {
            _rb.MovePosition(transform.position + transform.forward
                * (_input != Vector3.zero ? 1f : 0f)
                * _currentSpeed
                * Time.fixedDeltaTime);
        }

        if (jump && isGrounded && _pushedObject == null)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            AudioManager.instance.PlayOneShot(jumpSound, this.transform.position);

            jump = false;
            isGrounded = false;
        }
    }
}
