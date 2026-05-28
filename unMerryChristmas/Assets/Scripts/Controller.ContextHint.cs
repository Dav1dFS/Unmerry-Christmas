using UnityEngine;

public partial class Controller
{
    // Tracks which object is currently outlined so we can remove the highlight
    // when the player moves away or a different object becomes closest.
    private InteractableHighlight _currentHighlight;

    private void UpdateContextHint()
    {
        // Suppress hints and outlines while the player's hands are busy
        if (heldObject != null || _pushedObject != null || _giftInHand != null)
        {
            UIManager.Instance?.HideContextHint();
            ClearHighlight();
            return;
        }

        // ── Find closest interactable in pickup range ─────────────────────────
        Collider[]    hits        = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);
        IInteractable closest     = null;
        float         closestDist = Mathf.Infinity;
        bool          nearPushable = false;
        Collider      closestHit   = null;

        foreach (Collider hit in hits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d >= closestDist) continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                closest      = interactable;
                closestDist  = d;
                closestHit   = hit;
            }
        }

        // ── Also check for pushables in the (smaller) push range ──────────────
        Collider[] pushHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);
        foreach (Collider hit in pushHits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < closestDist && hit.GetComponent<PushableObject>() != null)
            {
                nearPushable = true;
                closest      = null;
                closestHit   = hit;
                closestDist  = d;
            }
        }

        // ── Context hint text ─────────────────────────────────────────────────
        if (nearPushable)
            UIManager.Instance?.ShowContextHint("Hold E — Push");
        else if (closest != null)
            UIManager.Instance?.ShowContextHint(closest.GetHintText());
        else
            UIManager.Instance?.HideContextHint();

        // ── Object outline (InteractableHighlight) ────────────────────────────
        // Find the InteractableHighlight on the closest object (if any).
        // We search up the hierarchy so the component can live on the root even
        // when the collider is on a child mesh.
        InteractableHighlight newHighlight = closestHit != null
            ? closestHit.GetComponentInParent<InteractableHighlight>()
            : null;

        if (newHighlight != _currentHighlight)
        {
            // Un-highlight the previous object
            _currentHighlight?.SetHighlighted(false);
            // Highlight the new closest one
            newHighlight?.SetHighlighted(true);
            _currentHighlight = newHighlight;
        }
    }

    /// <summary>Clears any active outline immediately (e.g. when hands become busy).</summary>
    private void ClearHighlight()
    {
        if (_currentHighlight == null) return;
        _currentHighlight.SetHighlighted(false);
        _currentHighlight = null;
    }
}
