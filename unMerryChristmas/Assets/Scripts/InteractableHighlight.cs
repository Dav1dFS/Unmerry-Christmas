using UnityEngine;

/// <summary>
/// Drives the LineworkLite FreeOutline highlight on any interactable object.
/// See original header for full setup documentation.
///
/// Accessibility addition: subscribes to AccessibilityManager.OnHighlightChanged.
/// When highlights are disabled globally, any active outline is cleared immediately
/// and future SetHighlighted(true) calls are blocked until re-enabled.
/// </summary>
public class InteractableHighlight : MonoBehaviour
{
    [Header("Ability Requirement")]
    [SerializeField] private bool requiresAbility = false;
    [SerializeField] private PlayerAbility requiredAbility;

    // Rendering layer index 2 = "Light Layer 2" in TagManager.
    // This matches FreeOutline's Outline slot 2 (RenderingLayer.m_Bits = 4 = 1 << 2).
    private const int OutlineLayerIndex = 2;
    private static readonly uint OutlineLayerBit = 1u << OutlineLayerIndex;

    private Renderer[] _renderers;
    private bool       _highlighted;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
        foreach (Renderer r in _renderers)
            if (r != null) r.renderingLayerMask &= ~OutlineLayerBit;
    }

    private void OnEnable()
    {
        AccessibilityManager.OnHighlightChanged += OnGlobalToggle;
        // Enforce current setting immediately in case manager exists and is off
        if (AccessibilityManager.Instance != null && !AccessibilityManager.Instance.HighlightInteractables)
            SetHighlighted(false);
    }

    private void OnDisable()
    {
        AccessibilityManager.OnHighlightChanged -= OnGlobalToggle;
        SetHighlighted(false);   // existing safety clear — preserved
    }

    private void OnDestroy() => SetHighlighted(false);

    /// <summary>
    /// Adds or removes this object from FreeOutline's rendering layer.
    /// Called by Controller.ContextHint every frame when the closest interactable changes.
    /// If highlights are globally disabled, SetHighlighted(true) is silently ignored.
    /// </summary>
    public void SetHighlighted(bool on)
    {
        // Block highlight if globally disabled
        if (on && AccessibilityManager.Instance != null
               && !AccessibilityManager.Instance.HighlightInteractables)
            return;

        if (on && requiresAbility)
        {
            if (AbilityTokenManager.Instance == null || !AbilityTokenManager.Instance.IsUnlocked(requiredAbility))
            {
                on = false;
            }
        }

        if (_highlighted == on) return;
        _highlighted = on;

        foreach (Renderer r in _renderers)
        {
            if (r == null) continue;
            if (on) r.renderingLayerMask |=  OutlineLayerBit;
            else    r.renderingLayerMask &= ~OutlineLayerBit;
        }
    }

    private void OnGlobalToggle(bool enabled)
    {
        if (!enabled) SetHighlighted(false);
        // When re-enabled, the next Controller.ContextHint frame will re-apply naturally
    }
}
