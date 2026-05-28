using UnityEngine;

public partial class Controller
{
    private void UpdateContextHint()
    {
        // Suppress hints while the player's hands are busy
        if (heldObject != null || _pushedObject != null || _giftInHand != null)
        {
            UIManager.Instance?.HideContextHint();
            return;
        }

        // Scan for the closest interactable within pickup range
        Collider[] hits       = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);
        IInteractable closest = null;
        float closestDist     = Mathf.Infinity;
        bool  nearPushable    = false;

        foreach (Collider hit in hits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d >= closestDist) continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                closest     = interactable;
                closestDist = d;
            }
        }

        // Also check for pushable objects in their (smaller) push range
        Collider[] pushHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);
        foreach (Collider hit in pushHits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < closestDist && hit.GetComponent<PushableObject>() != null)
            {
                nearPushable = true;
                closest      = null;  // pushable wins over interactable at shorter range
                closestDist  = d;
            }
        }

        if (nearPushable)
            UIManager.Instance?.ShowContextHint("Hold E — Push");
        else if (closest != null)
            UIManager.Instance?.ShowContextHint(closest.GetHintText());
        else
            UIManager.Instance?.HideContextHint();
    }
}
