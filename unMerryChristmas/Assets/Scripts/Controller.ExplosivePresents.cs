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
    }

    private void ThrowGift()
    {
        if (_giftInHand == null) return;

        ExplosivePresent gift = _giftInHand;
        _giftInHand = null;

        gift.transform.SetParent(null);
        gift.Arm();

        Collider col = gift.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = gift.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            float   force    = Mathf.Clamp(holdTime * throwForce, 5f, 15f);
            Vector3 throwDir = transform.forward + Vector3.up * 0.5f;
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
