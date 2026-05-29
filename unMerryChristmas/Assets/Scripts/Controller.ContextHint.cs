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

        // ── Find closest interactable of ANY type in pickup range ─────────────
        // This covers IInteractable (ShedDoor, MovingBox…), PickupObject (stones,
        // gnomes…), DrawingPageCollectable, and AbilityToken — all of which share
        // the pickupLayer but do NOT necessarily implement IInteractable.
        Collider[] hits        = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);
        string     hintText    = null;
        float      closestDist = Mathf.Infinity;
        Collider   closestHit  = null;

        foreach (Collider hit in hits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d >= closestDist) continue;

            string text = GetHintForCollider(hit);
            if (text == null) continue;

            hintText    = text;
            closestDist = d;
            closestHit  = hit;
        }

        // ── Also check for pushables in the (smaller) push range ──────────────
        Collider[] pushHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);
        foreach (Collider hit in pushHits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < closestDist && hit.GetComponent<PushableObject>() != null)
            {
                hintText    = "Hold E — Push";
                closestHit  = hit;
                closestDist = d;
            }
        }

        // ── Context hint text ─────────────────────────────────────────────────
        if (hintText != null)
            UIManager.Instance?.ShowContextHint(hintText);
        else
            UIManager.Instance?.HideContextHint();

        // ── Object outline (InteractableHighlight) ────────────────────────────
        // Walk up the hierarchy so the component can live on the root even
        // when the collider is on a child mesh.
        InteractableHighlight newHighlight = closestHit != null
            ? closestHit.GetComponentInParent<InteractableHighlight>()
            : null;

        if (newHighlight != _currentHighlight)
        {
            _currentHighlight?.SetHighlighted(false);
            newHighlight?.SetHighlighted(true);
            _currentHighlight = newHighlight;
        }
    }

    /// <summary>
    /// Returns the context hint string for any collider the player can interact with,
    /// or null if this collider represents nothing actionable.
    /// Covers all interaction paths: IInteractable, pickups, collectables, tokens.
    /// </summary>
    private string GetHintForCollider(Collider col)
    {
        // IInteractable objects provide their own text (ShedDoor, MovingBox, etc.)
        IInteractable interactable = col.GetComponent<IInteractable>();
        if (interactable != null) return interactable.GetHintText();

        // Ability tokens and drawing-page collectables (tag-based detection)
        if (col.CompareTag("Token"))       return "E — Collect";
        if (col.CompareTag("Collectable")) return "E — Collect";

        // Physical pickups (stones, gnomes, candy, …)
        if (col.GetComponent<PickupObject>() != null)
        {
            bool canThrow = AbilityTokenManager.Instance != null
                         && AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing);
            return canThrow ? "E — Pick up  |  Hold E — Throw" : "E — Pick up";
        }

        return null;
    }

    /// <summary>Clears any active outline immediately (e.g. when hands become busy).</summary>
    private void ClearHighlight()
    {
        if (_currentHighlight == null) return;
        _currentHighlight.SetHighlighted(false);
        _currentHighlight = null;
    }
}
