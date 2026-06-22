/// <summary>
/// Lifecycle state of a single task in the scene-agnostic task system
/// (<see cref="TaskService"/>). Mirrors the states the Backyard uses, but lives
/// outside any per-scene tracker so every scene can share the same vocabulary.
/// </summary>
public enum TaskState
{
    /// <summary>Playable now; can be completed.</summary>
    Available,

    /// <summary>Not yet playable — waiting on a required ability (see TaskDefinition).</summary>
    Locked,

    /// <summary>Done.</summary>
    Completed,
}
