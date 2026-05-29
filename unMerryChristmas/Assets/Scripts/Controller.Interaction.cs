using FMODUnity;
using UnityEngine;

/// <summary>
/// Pickup, drop, push, aim, throw, and throw-trajectory systems.
/// Push and pickup/throw share so much state they live in the same file.
/// </summary>
public partial class Controller
{
    // ── Settings ─────────────────────────────────────────────────────────────
    [Header("Interaction — Pickup & Push")]
    [SerializeField] private float pickupRange   = 1.5f;
    [SerializeField] private float pushableRange = 0.8f;

    [Header("Interaction — Throw")]
    [SerializeField] private float throwForce         = 10f;
    [SerializeField] private float aimHoldThreshold   = 0.2f;
    [SerializeField] private float maxThrowChargeTime = 2f;

    [Header("Interaction — Trajectory")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int          trajectoryPoints   = 30;
    [SerializeField] private float        trajectoryTimeStep = 0.1f;
    [SerializeField] private LayerMask    trajectoryCollisionMask;

    [Header("Interaction — Audio")]
    [SerializeField] private EventReference grabSound;
    [SerializeField] private EventReference throwSound;
    [SerializeField] private EventReference chargeThrowSound;

    // ── State ────────────────────────────────────────────────────────────────
    private PickupObject   heldObject;
    private PushableObject _pushedObject;
    private Vector3        _contactLocalPos;
    private Vector3        _playerLocalPos;

    private bool  isAiming          = false;
    private float holdTime          = 0f;
    private bool  isHoldingInteract = false;
    private bool  hasEnteredAimMode = false;

    private FMOD.Studio.EventInstance chargeThrowInstance;

    // ── Per-frame update ──────────────────────────────────────────────────────

    private void UpdateInteraction()
    {
        // Accumulate hold time and enter aim mode when threshold is reached
        if (isHoldingInteract && heldObject != null)
        {
            holdTime += Time.deltaTime;

            if (!hasEnteredAimMode && holdTime >= aimHoldThreshold
                && AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing))
            {
                hasEnteredAimMode   = true;
                isAiming            = true;
                chargeThrowInstance = RuntimeManager.CreateInstance(chargeThrowSound);
                RuntimeManager.AttachInstanceToGameObject(chargeThrowInstance, transform, _rb);
                chargeThrowInstance.start();
            }
        }

        // Trajectory arc
        if (isAiming)
            DrawTrajectory();
        else
            trajectoryLine.enabled = false;

        // Release pushed object if player walks too far away
        if (_pushedObject != null)
        {
            if (!_pushedObject.IsWithinPushDistance())
                ReleasePushable();
            else
                ClearAbilityInputs();
        }
    }

    // ── Interact hold / release ───────────────────────────────────────────────

    private void StartInteractHold()
    {
        if (_pushedObject != null) return;
        isHoldingInteract = true;
        hasEnteredAimMode = false;
        holdTime          = 0f;
    }

    private void ReleaseInteractHold()
    {
        if (!isHoldingInteract) return;
        isHoldingInteract = false;

        if (hasEnteredAimMode)
        {
            // Release after charging → throw
            if (heldObject != null) throwObject();
            isAiming = false;
            holdTime = 0f;
            StopChargeSound();
        }
        else
        {
            // Tap → context-sensitive hand action
            checkHands();
        }
    }

    // ── Hands (context-sensitive tap) ────────────────────────────────────────

    private void checkHands()
    {
        if      (heldObject    != null) DropObject();
        else if (_pushedObject != null) ReleasePushable();
        else if (_giftInHand   != null) DropGift();
        else                            TryPickup();
    }

    // ── Pickup ───────────────────────────────────────────────────────────────

    private void TryPickup()
    {
        Collider[] pickupHits   = Physics.OverlapSphere(transform.position, pickupRange,   pickupLayer);
        Collider[] pushableHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);

        Collider[] hits = new Collider[pickupHits.Length + pushableHits.Length];
        pickupHits.CopyTo(hits, 0);
        pushableHits.CopyTo(hits, pickupHits.Length);

        float          closestDist        = Mathf.Infinity;
        PickupObject   closestPickup      = null;
        Collider       closestCollectable = null;
        PushableObject closestPushable    = null;

        foreach (Collider hit in hits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d >= closestDist) continue;

            if (hit.CompareTag("Collectable") || hit.CompareTag("Token"))
            {
                closestDist        = d;
                closestCollectable = hit;
                closestPickup      = null;
                closestPushable    = null;
            }
            else
            {
                PickupObject pickup = hit.GetComponent<PickupObject>();
                if (pickup != null)
                {
                    closestDist     = d;
                    closestPickup   = pickup;
                    closestPushable = null;
                }
                else
                {
                    IInteractable interactable = hit.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        // Interact immediately — no further checks needed
                        closestDist = d;
                        interactable.Interact();
                        return;
                    }

                    PushableObject pushable = hit.GetComponent<PushableObject>();
                    if (pushable != null)
                    {
                        closestDist     = d;
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
                if (token != null) AbilityTokenManager.Instance.Unlock(token.Ability);
            }
            else
            {
                CollectableManager.Instance.Collect();
            }
            Destroy(closestCollectable.gameObject);
        }
        else if (closestPickup != null)
        {
            heldObject = closestPickup;
            heldObject.OnPickup(holdPoint);
            AudioManager.instance?.PlayOneShot(grabSound, transform.position);
        }
        else if (closestPushable != null)
        {
            if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Pushing))
                return;

            // Find precise contact point on the pushable's surface via raycast
            Vector3 dirToObj = (closestPushable.transform.position - transform.position).normalized;
            Vector3 contactWorldPos;
            if (Physics.Raycast(transform.position, dirToObj, out RaycastHit contactHit, pickupRange * 2f, pushableLayer))
                contactWorldPos = contactHit.point;
            else
                contactWorldPos = closestPushable.GetComponent<Collider>().ClosestPoint(transform.position);

            _pushedObject    = closestPushable;
            _contactLocalPos = closestPushable.transform.InverseTransformPoint(contactWorldPos);
            _playerLocalPos  = closestPushable.transform.InverseTransformPoint(transform.position);
            _pushedObject.OnGrab();
        }
    }

    // ── Throw ─────────────────────────────────────────────────────────────────

    private void throwObject()
    {
        if (heldObject == null) return;

        isAiming = false;
        float charge = Mathf.Clamp01((holdTime - aimHoldThreshold) / maxThrowChargeTime);
        float force  = Mathf.Lerp(5f, 15f, charge);

        Rigidbody objectRb = heldObject.GetComponent<Rigidbody>();
        if (objectRb != null)
        {
            ThrowableImpact impact = heldObject.GetComponent<ThrowableImpact>();
            if (impact != null) impact.SetThrown();

            DropObject();
            Vector3 throwDir = transform.forward + Vector3.up * 0.5f;
            objectRb.AddForce(throwDir.normalized * force, ForceMode.Impulse);
            AudioManager.instance?.PlayOneShot(throwSound, transform.position);
        }
    }

    private void StopChargeSound()
    {
        if (!chargeThrowInstance.isValid()) return;
        chargeThrowInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        chargeThrowInstance.release();
        chargeThrowInstance.clearHandle();
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

        float charge         = Mathf.Clamp01((holdTime - aimHoldThreshold) / maxThrowChargeTime);
        float force          = Mathf.Lerp(5f, 15f, charge);
        Vector3 startPos     = holdPoint.position;
        Vector3 startVel     = (transform.forward + Vector3.up * 0.5f).normalized * force;
        Vector3 prevPoint    = startPos;

        trajectoryLine.positionCount = trajectoryPoints;
        int count = 0;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float   t     = i * trajectoryTimeStep;
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
}
