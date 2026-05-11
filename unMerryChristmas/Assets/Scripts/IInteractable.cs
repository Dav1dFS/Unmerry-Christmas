public interface IInteractable
{
    void Interact();

    // Default hint shown by ContextualHint when player is near this object.
    // Override in implementing classes for a more descriptive prompt.
    string GetHintText() => "E — Interact";
}
