using UnityEngine;

/// <summary>
/// The drawing book KEY ITEM found in the Back Yard.
///
/// Unlike drawing pages (tag "Collectable" → CollectableManager) and ability
/// tokens (tag "Token" → AbilityTokenManager), the book is found exactly once
/// and must NOT count toward the 20 collectable pages. It is picked up through
/// the <see cref="IInteractable"/> path in Controller.TryPickup, so this object
/// should be on the Pickable layer, NOT tagged "Collectable"/"Token", and carry
/// no enabled <see cref="PickupObject"/> (otherwise it would be grabbed/carried).
///
/// On pickup (Level Design Document §3.3):
///   1. Records the book as found (<see cref="BookState"/>).
///   2. Plays the elf's introductory animation.
///   3. Shows the "New note added" toast and opens the book to the Tutorial page.
///   4. Removes the world object — the elf now carries the book.
///
/// Wire it up automatically with  Tools ▸ UnMerry ▸ Setup Drawing Book Pickup.
/// </summary>
[DisallowMultipleComponent]
public class DrawingBookPickup : MonoBehaviour, IInteractable
{
    [Tooltip("Hint shown when the elf is near the book.")]
    [SerializeField] private string _hintText = "E — Pick up the drawing book";

    private bool _collected;

    public void Interact()
    {
        if (_collected) return;
        _collected = true;

        BookState.MarkFound();
        AudioManager.instance?.PlayUnlockAbilitySound(transform.position);

        // Introductory animation on the elf (graceful no-op if the animator
        // has no "PickupBook" trigger yet — see Controller.PlayBookPickupAnimation).
        var controller = FindFirstObjectByType<Controller>();
        controller?.PlayBookPickupAnimation();

        // HUD toast + auto-open the book to the Tutorial page. Routed through the
        // persistent UIManager so the sequence survives this object's destruction.
        UIManager.Instance?.OnDrawingBookFound();

        // The elf now carries the book; remove it from the world.
        Destroy(gameObject);
    }

    public string GetHintText() => _hintText;
}
