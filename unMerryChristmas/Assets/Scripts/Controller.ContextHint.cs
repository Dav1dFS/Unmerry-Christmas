using UnityEngine;
using UnityEngine.InputSystem;

public partial class Controller
{
    private InteractableHighlight _currentHighlight;

    private void UpdateContextHint()
    {
        if (heldObject != null || _pushedObject != null || _giftInHand != null)
        {
            UIManager.Instance?.HideContextHint();
            ClearHighlight();
            return;
        }

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

        Collider[] pushHits = Physics.OverlapSphere(transform.position, pushableRange, pushableLayer);
        foreach (Collider hit in pushHits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < closestDist && hit.GetComponent<PushableObject>() != null)
            {
                hintText   = "Hold E — Push";
                closestHit = hit;
                closestDist = d;
            }
        }

        if (closestHit != null
            && AccessibilityManager.Instance != null
            && AccessibilityManager.Instance.HintsEnabled
            && _hint != null
            && _hint.IsPressed())
        {
            var provider = closestHit.GetComponentInParent<HintProvider>();
            if (provider != null && !string.IsNullOrEmpty(provider.HintText))
                hintText = provider.HintText;
        }

        if (hintText != null)
            UIManager.Instance?.ShowContextHint(hintText);
        else
            UIManager.Instance?.HideContextHint();

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

    private string GetHintForCollider(Collider col)
    {
        IInteractable interactable = col.GetComponent<IInteractable>();
        if (interactable != null) return interactable.GetHintText();

        if (col.CompareTag("Token"))       return "E — Collect";
        if (col.CompareTag("Collectable")) return "E — Collect";

        if (col.GetComponent<PickupObject>() != null)
        {
            bool canThrow = AbilityTokenManager.Instance != null
                         && AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing);
            return canThrow ? "E — Pick up  |  Hold E — Throw" : "E — Pick up";
        }

        return null;
    }

    private void ClearHighlight()
    {
        if (_currentHighlight == null) return;
        _currentHighlight.SetHighlighted(false);
        _currentHighlight = null;
    }

    private void OnHighlightSettingChanged(bool enabled) => _currentHighlight = null;
}
