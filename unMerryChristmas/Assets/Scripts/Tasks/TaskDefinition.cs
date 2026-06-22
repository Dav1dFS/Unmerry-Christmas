using UnityEngine;

/// <summary>
/// ScriptableObject identity for one task. Each scene owns its own set of these
/// assets, decoupling the task system from any per-scene enum (unlike the
/// Backyard's <c>BackYardTaskTracker.TaskId</c>). A task's identity is the asset
/// reference itself, which is stable across scenes and survives serialisation —
/// handy for the future save system.
/// </summary>
[CreateAssetMenu(menuName = "UnMerry/Task Definition", fileName = "Task_")]
public class TaskDefinition : ScriptableObject
{
    [Tooltip("Name shown in the drawing-book Task List and the 'Task Complete' flash.")]
    [SerializeField] private string _displayName = "New Task";

    [Tooltip("If true the task starts Locked and only becomes Available once the " +
             "Unlock Ability below is unlocked. If false the task is Available from the start.")]
    [SerializeField] private bool _lockedByDefault = false;

    [Tooltip("Ability that unlocks this task. Only used when 'Locked By Default' is true.")]
    [SerializeField] private PlayerAbility _unlockAbility = PlayerAbility.ExplosingPresents;

    public string DisplayName => _displayName;
    public bool LockedByDefault => _lockedByDefault;
    public PlayerAbility UnlockAbility => _unlockAbility;
}
