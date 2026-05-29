using System;
using System.Collections.Generic;
using UnityEngine;

public class BackYardTaskTracker : MonoBehaviour
{
    public enum TaskId
    {
        MainClimbWindow,
        GardenChaos,
        GnomeParade,
        UnearthThePast,
        LightsOut,
        BurstPipe,
        IceSlide,
        BirdbathDemolition,
        ShedHeist,
        TheHighWall,
    }

    public enum TaskState { Available, Locked, Completed }

    public static BackYardTaskTracker Instance { get; private set; }

    // Subscribe here to react to task completions (e.g. HUD flash)
    public static event Action<TaskId> OnTaskCompleted;

    private readonly Dictionary<TaskId, TaskState> _states = new()
    {
        { TaskId.MainClimbWindow,    TaskState.Available },
        { TaskId.GardenChaos,        TaskState.Available },
        { TaskId.GnomeParade,        TaskState.Available },
        { TaskId.UnearthThePast,     TaskState.Available },
        { TaskId.LightsOut,          TaskState.Available },
        // Cross-scenario: require abilities from later levels
        { TaskId.BurstPipe,          TaskState.Locked },
        { TaskId.IceSlide,           TaskState.Locked },
        { TaskId.BirdbathDemolition, TaskState.Locked },
        { TaskId.ShedHeist,          TaskState.Locked },
        { TaskId.TheHighWall,        TaskState.Locked },
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AbilityTokenManager.OnAbilityUnlocked += HandleAbilityUnlocked;
    }

    private void OnDestroy()
    {
        AbilityTokenManager.OnAbilityUnlocked -= HandleAbilityUnlocked;
        if (Instance == this) Instance = null;
    }

    private void HandleAbilityUnlocked(PlayerAbility ability)
    {
        if (ability == PlayerAbility.Throwing)
        {
            UnlockTask(TaskId.BurstPipe);
            UnlockTask(TaskId.BirdbathDemolition);
            UnlockTask(TaskId.ShedHeist);
            // IceSlide unlocks only after BurstPipe is completed (handled in IceSlideTracker)
        }
        if (ability == PlayerAbility.Ziplining)
            UnlockTask(TaskId.TheHighWall);
    }
    public static void UnlockTask(TaskId id)
    {
        if (Instance == null) return;
        if (Instance._states.TryGetValue(id, out var state) && state == TaskState.Locked)
        {
            Instance._states[id] = TaskState.Available;
        }
    }
    public static void ReportTask(TaskId id)
    {
        if (Instance == null) return;
        if (Instance._states[id] != TaskState.Available) return;

        Instance._states[id] = TaskState.Completed;
        OnTaskCompleted?.Invoke(id);
    }

    public static bool IsCompleted(TaskId id)
    {
        return Instance != null
            && Instance._states.TryGetValue(id, out var s)
            && s == TaskState.Completed;
    }

    public static bool IsAvailable(TaskId id)
    {
        return Instance != null
            && Instance._states.TryGetValue(id, out var s)
            && s == TaskState.Available;
    }
}
