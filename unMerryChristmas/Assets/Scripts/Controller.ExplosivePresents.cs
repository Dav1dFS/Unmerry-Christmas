using UnityEngine;

public partial class Controller
{
    [Header("Explosive Presents")]
    [SerializeField] private GameObject _giftBombPrefab;

    private ExplosivePresent _giftInHand;

    private void TrySpawnGift()
    {
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.ExplosingPresents))
            return;
        if (heldObject != null || _giftInHand != null) return;

        GameObject obj = Instantiate(_giftBombPrefab, holdPoint.position, Quaternion.identity);
        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        _giftInHand = obj.GetComponent<ExplosivePresent>();

        // Disable collider while held so it doesn't collide with the player
        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // FIX: Force the interaction state to immediately register this gift if the player 
        // spawned it while holding down an input or if they immediately transition into a hold state.
        if (isHoldingInteract)
        {
            hasEnteredAimMode = false;
            holdTime = 0f;
        }
    }

    private void ThrowGift()
    {
        if (_giftInHand == null) return;

        ExplosivePresent gift = _giftInHand;
        _giftInHand = null;

        // FIX: Offset forward to clear the player's collision bounds perfectly
        gift.transform.position = holdPoint.position + transform.forward * 0.2f;
        gift.transform.SetParent(null);
        gift.Arm();

        Collider col = gift.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = gift.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;

            // FIX: Changes force scaling to use cachedCharge and your adjustable throwForce field
            float force = Mathf.Lerp(3f, throwForce, cachedCharge);
            
            Vector3 throwDir = transform.forward + Vector3.up * 0.4f; 
            rb.AddForce(throwDir.normalized * force, ForceMode.Impulse);
            AudioManager.instance?.PlayThrowSound(transform.position);
        }

        isAiming = false;
        holdTime = 0f;
        StopChargeSound();
    }

    private void DropGift()
    {
        if (_giftInHand == null) return;

        ExplosivePresent gift = _giftInHand;
        _giftInHand = null;

        gift.transform.SetParent(null);
        gift.Arm();

        Collider col = gift.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = gift.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }
}