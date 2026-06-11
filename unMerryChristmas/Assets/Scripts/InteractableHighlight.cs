using UnityEngine;

/// <summary>
/// Drives the LineworkLite FreeOutline highlight on any interactable object.
///
/// HOW IT WORKS
/// ─────────────
/// The project uses LineworkLite FreeOutline (URP Renderer Feature) configured in
/// "Free Outline Settings.asset".  Outline slot 2 is set to rendering layer index 2
/// ("Light Layer 2", m_Bits = 4).  Objects are normally on rendering layer 0 (Default).
///
/// When SetHighlighted(true) is called, this component adds rendering layer 2 to every
/// child Renderer on the object.  FreeOutline picks that up and draws the outline.
/// SetHighlighted(false) removes the layer, making the outline disappear.
///
/// SETUP
/// ─────
/// Add this component to the root GameObject of any interactable object whose
/// mesh(es) should glow when the player is nearby.  No further Inspector wiring
/// is needed — the component caches its own renderers on Awake.
///
/// The Controller already calls SetHighlighted via Controller.ContextHint.cs, which
/// tracks the closest IInteractable each frame.
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
        // Cache all renderers including children (e.g. multi-mesh objects).
        // includeInactive: false — don't outline parts that are disabled at start.
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: false);

        // IMPORTANT: Unity's default renderingLayerMask is 0xFFFFFFFF (all bits set),
        // which means bit 2 is already on every renderer before we touch anything.
        // FreeOutline Outline 2 watches for bit 2 — so without this strip every object
        // would be permanently outlined the moment Outline 2 is active.
        // We clear bit 2 here so objects start without the highlight, and SetHighlighted
        // adds / removes it dynamically as the player looks around.
        foreach (Renderer r in _renderers)
        {
            if (r != null) r.renderingLayerMask &= ~OutlineLayerBit;
        }
    }

    /// <summary>
    /// Adds or removes this object from FreeOutline's rendering layer.
    /// Called by Controller.ContextHint every frame when the closest interactable changes.
    /// </summary>
    public void SetHighlighted(bool on)
    {
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
            if (on)
                r.renderingLayerMask |= OutlineLayerBit;
            else
                r.renderingLayerMask &= ~OutlineLayerBit;
        }
    }

    // Safety: ensure we un-highlight when the object is disabled or destroyed
    // so no phantom outlines are left in the renderer.
    private void OnDisable()  => SetHighlighted(false);
    private void OnDestroy()  => SetHighlighted(false);
}