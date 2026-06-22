using FMODUnity;
using UnityEngine;
using System.Collections;

public partial class Controller
{
    // ── Settings ─────────────────────────────────────────────────────────────
    [Header("Interaction — Pickup & Push")]
    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private float pushableRange = 0.1f;

    [Tooltip("Vertical offset (world units) added to the player position when centering the " +
             "pickup/interaction detection sphere.\n\n" +
             "DEFAULT is 0 (sphere at the player's feet) — correct for backyard and any scene " +
             "where interactables sit on the ground.\n\n" +
             "Set to ~0.7 on the kitchen Player instance so the sphere reaches items resting " +
             "on counters or tables above the player's head. Select the Player to see the " +
             "detection sphere gizmo and tune this value against the scene geometry.")]
    [SerializeField] private float interactionHeightOffset = 0f;  // FIX: was 0.7f — that default
                                                                  // raised the sphere off the floor
                                                                  // and broke pickup for backyard
                                                                  // items. Override to 0.7f on the
                                                                  // kitchen Player in the inspector.

    [Header("Interaction — Throw")]
    [SerializeField] private float throwAnimationDelay = 0.15f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float aimHoldThreshold = 0.2f;
    [SerializeField] private float maxThrowChargeTime = 2f;

    [Header("Interaction — Trajectory")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    [SerializeField] private LayerMask trajectoryCollisionMask;

    [Header("Interaction — Audio")]
    [SerializeField] private EventReference chargeThrowSound;

    // ── State ────────────────────────────────────────────────────────────────
    private PickupObject heldObject;
    private PushableObject _pushedObject;
    private Vector3 _contactLocalPos;
    private Vector3 _playerWorldOffset;

    private bool isAiming = false;
    private float holdTime = 0f;
    private bool isHoldingInteract = false;
    private bool hasEnteredAimMode = false;
    private float cachedCharge = 0f;

    private FMOD.Studio.EventInstance chargeThrowInstance;

    // Center of the pickup/interaction detection sphere. Offset up from the player
    // by interactionHeightOffset (default 0 — sphere at feet, safe for all scenarios).
    // Set interactionHeightOffset > 0 in the inspector on any Player whose scene has
    // interactables above ground level (e.g. kitchen counters).
    // The pickup logic and the contextual hint MUST use this same center so the "E — …"
    // prompt and the action that follows always agree.
    private Vector3 InteractionCenter => transform.position + Vector3.up * interactionHeightOffset;

    // ── Per-frame update ──────────────────────────────────────────────────────

    private void UpdateInteraction()
    {
        if (isHoldingInteract)
        {
            holdTime += Time.deltaTime;

            if (_pushedObject != null)
            {
                // Already pushing — nothing to do
            }
            else if (heldObject != null || _giftInHand != null)
            {
                // Held object or gift — enter aim mode on hold
                // FIX: Ensured both objects and gifts pass cleanly into aiming
                if (!hasEnteredAimMode && holdTime >= aimHoldThreshold)
                {
                    hasEnteredAimMode = true;
                    isAiming = true;
                    if (AudioManager.instance != null)
                    {
                        chargeThrowInstance = AudioManager.instance.CreateInstance(chargeThrowSound);
                        AudioManager.instance.AttachInstanceToGameObject(chargeThrowInstance, transform, _rb);
                        chargeThrowInstance.start();
                    }
                }
            }
            else if (holdTime >= aimHoldThreshold)
            {
                // Nothing in hand — try to grab pushable
                TryPushGrab();
            }
        }

        if (isAiming)
            DrawTrajectory();
        else
            trajectoryLine.enabled = false;

        if (_pushedObject != null)
            ClearAbilityInputs();
    }

    // ── Interact hold / release ───────────────────────────────────────────────

    private void StartInteractHold()
    {
        isHoldingInteract = true;
        hasEnteredAimMode = false;
        holdTime = 0f;
    }

    private void ReleaseInteractHold()
    {
        if (!isHoldingInteract) return;
        isHoldingInteract = false;

        if (_pushedObject != null)
        {
            ReleasePushable();
            return;
        }

        if (hasEnteredAimMode)
        {
            cachedCharge = Mathf.Clamp01((holdTime - aimHoldThreshold) / maxThrowChargeTime);

            _animator.SetTrigger("Throw");

            if (heldObject != null)
                StartCoroutine(ThrowObjectDelayed());
            else if (_giftInHand != null)
                StartCoroutine(ThrowGiftDelayed());

            isAiming = false;
            holdTime = 0f;
            StopChargeSound();
        }
        else
        {
            checkHands();
        }
    }

    // ── Hands (context-sensitive tap) ────────────────────────────────────────

    private void checkHands()
    {
        if (heldObject != null) DropObject();
        else if (_pushedObject != null) ReleasePushable();
        else if (_giftInHand != null) DropGift();
        else TryPickup();
    }

    // ── Pickup ───────────────────────────────────────────────────────────────

    private void TryPickup()
    {
        Collider[] pickupHits = Physics.OverlapSphere(InteractionCenter, pickupRange, pickupLayer);
        float closestDist = Mathf.Infinity;
        PickupObject closestPickup = null;
        Collider closestCollectable = null;

        foreach (Collider hit in pickupHits)
        {
            float d = Vector3.Distance(InteractionCenter, hit.transform.position);
            if (d >= closestDist) continue;

            if (hit.CompareTag("Collectable") || hit.CompareTag("Token"))
            {
                closestDist = d;
                closestCollectable = hit;
                closestPickup = null;
            }
            else
            {
                PickupObject pickup = hit.GetComponent<PickupObject>();
                if (pickup != null && pickup.enabled)
                {
                    closestDist = d;
                    closestPickup = pickup;
                }
                else
                {
                    // GetComponentInParent so interactables whose colliders live on
                    // child meshes (e.g. the drawing book) still resolve.
                    IInteractable interactable = hit.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        closestDist = d;
                        interactable.Interact();
                        return;
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
                {
                    AbilityTokenManager.Instance.Unlock(token.Ability);
                    AudioManager.instance?.PlayUnlockAbilitySound(transform.position);
                }
            }
            else
            {
                CollectableManager.Instance.Collect();
                AudioManager.instance?.PlayUnlockAbilitySound(transform.position);
            }
            Destroy(closestCollectable.gameObject);
        }
        else if (closestPickup != null)
        {
            heldObject = closestPickup;
            heldObject.OnPickup(holdPoint);
            AudioManager.instance?.PlayGrabSound(transform.position);
        }
    }

    // ── Push Grab (Triggered by Hold) ────────────────────────────────────────

    private void TryPushGrab()
    {
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Pushing))
            return;

        Collider[] pushableHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);
        float closestDist = Mathf.Infinity;
        PushableObject closestPushable = null;

        foreach (Collider hit in pushableHits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d >= closestDist) continue;

            PushableObject pushable = hit.GetComponent<PushableObject>();
            if (pushable != null)
            {
                closestDist = d;
                closestPushable = pushable;
            }
        }

        if (closestPushable != null)
        {
            Vector3 toObj = closestPushable.transform.position - transform.position;
            Vector3 horizontalDir = new Vector3(toObj.x, 0f, toObj.z);
            if (horizontalDir.sqrMagnitude < 0.001f)
                horizontalDir = new Vector3(transform.forward.x, 0f, transform.forward.z);
            Vector3 dirToObj = horizontalDir.normalized;
            Vector3 rayOrigin = new Vector3(transform.position.x, closestPushable.transform.position.y, transform.position.z);
            Vector3 contactWorldPos;

            if (Physics.Raycast(rayOrigin, dirToObj, out RaycastHit contactHit, pickupRange * 2f, pushableLayer))
                contactWorldPos = contactHit.point;
            else
                contactWorldPos = closestPushable.GetComponent<Collider>().ClosestPoint(rayOrigin);

            _pushedObject = closestPushable;
            _contactLocalPos = closestPushable.transform.InverseTransformPoint(contactWorldPos);

            Vector3 offset = transform.position - closestPushable.transform.position;
            _playerWorldOffset = new Vector3(offset.x, 0f, offset.z);
            _pushedObject.OnGrab();
        }
    }

    // ── Throw ─────────────────────────────────────────────────────────────────

    private void throwObject()
    {
        if (heldObject == null) return;

        isAiming = false;

        // FIX 1: Max force scaled down using throwForce variable
        float force = Mathf.Lerp(3f, throwForce, cachedCharge);

        Rigidbody objectRb = heldObject.GetComponent<Rigidbody>();
        if (objectRb != null)
        {
            ThrowableImpact impact = heldObject.GetComponent<ThrowableImpact>();
            if (impact != null) impact.SetThrown();

            // FIX 2: Offset the object slightly forward on release so it doesn't
            // instantly collide with the player's body or hand layer.
            heldObject.transform.position = holdPoint.position + transform.forward * 0.2f;

            DropObject();

            Vector3 throwDir = transform.forward + Vector3.up * 0.4f;
            objectRb.AddForce(throwDir.normalized * force, ForceMode.Impulse);

            AudioManager.instance?.PlayThrowSound(transform.position);
        }
    }

    private IEnumerator ThrowObjectDelayed()
    {
        yield return new WaitForSeconds(throwAnimationDelay);
        throwObject();
    }

    private IEnumerator ThrowGiftDelayed()
    {
        yield return new WaitForSeconds(throwAnimationDelay);
        ThrowGift();
    }

    private void StopChargeSound()
    {
        AudioManager.instance?.StopAndReleaseInstance(chargeThrowInstance);
    }

    // ── Push ──────────────────────────────────────────────────────────────────

    private void ReleasePushable()
    {
        _pushedObject.OnRelease();
        _pushedObject = null;
    }

    // ── Drop ──────────────────────────────────────────────────────────────────

    private void DropObject()
    {
        heldObject.OnDrop();
        heldObject = null;
    }

    // ── Trajectory preview ────────────────────────────────────────────────────

    private void DrawTrajectory()
    {
        if (trajectoryLine == null) return;
        trajectoryLine.enabled = true;

        float charge = Mathf.Clamp01((holdTime - aimHoldThreshold) / maxThrowChargeTime);
        // FIX 1: Keep math matching throwObject()
        float force = Mathf.Lerp(3f, throwForce, charge);
        Vector3 startPos = holdPoint.position + transform.forward * 0.2f;
        Vector3 startVel = (transform.forward + Vector3.up * 0.4f).normalized * force;
        Vector3 prevPoint = startPos;

        trajectoryLine.positionCount = trajectoryPoints;
        int count = 0;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;
            Vector3 point = startPos + startVel * t + 0.5f * Physics.gravity * t * t;

            if (Physics.Linecast(prevPoint, point, out RaycastHit hit, trajectoryCollisionMask))
            {
                trajectoryLine.positionCount = count + 1;
                trajectoryLine.SetPosition(count, hit.point);
                break;
            }

            trajectoryLine.SetPosition(count, point);
            prevPoint = point;
            count++;
        }
    }

    // ── Editor visualisation ─────────────────────────────────────────────────
    // Draw the pickup/interaction detection sphere when the Player is selected so its
    // reach (and the interactionHeightOffset) can be tuned against the scene geometry.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * interactionHeightOffset, pickupRange);
    }
}