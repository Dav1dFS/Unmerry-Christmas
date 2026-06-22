using System;

/// <summary>
/// Global progression flag for the elf's drawing book.
///
/// The book is a one-time KEY ITEM found in the Back Yard (see Level Design
/// Document §3.3). After it is found, the elf "carries" it for the rest of the
/// game and it becomes the in-game menu / ability reference.
///
/// State is in-memory only — consistent with the rest of the game's runtime
/// managers (AbilityTokenManager, CollectableManager, BackYardTaskTracker),
/// which also hold session state without persistence. When a save system is
/// added (Assets/SaveFiles/_json.cs is currently a stub), serialise
/// <see cref="HasFoundBook"/> alongside the unlocked abilities and restore it
/// via <see cref="SetFound"/>.
/// </summary>
public static class BookState
{
    /// <summary>True once the elf has picked up the drawing book.</summary>
    public static bool HasFoundBook { get; private set; }

    /// <summary>Fires once, the first time the book is found.</summary>
    public static event Action OnBookFound;

    /// <summary>Marks the book as found and notifies listeners (idempotent).</summary>
    public static void MarkFound()
    {
        if (HasFoundBook) return;
        HasFoundBook = true;
        OnBookFound?.Invoke();
    }

    /// <summary>Restore persisted state without firing the found event.</summary>
    public static void SetFound(bool found) => HasFoundBook = found;

    /// <summary>Clear state (e.g. when starting a brand-new game).</summary>
    public static void Reset() => HasFoundBook = false;
}
