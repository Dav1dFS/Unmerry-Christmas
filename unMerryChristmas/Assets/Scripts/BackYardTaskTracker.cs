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
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Called by task scripts when they complete their objective.
    /// Silently ignored for Locked or already-Completed tasks.
    /// </summary>
    public static void ReportTask(TaskId id)
    {
        if (Instance == null) return;
        if (Instance._states[id] != TaskState.Available) return;

        Instance._states[id] = TaskState.Completed;
        Debug.Log($"[BackYardTaskTracker] Completed: {id}");
        OnTaskCompleted?.Invoke(id);
    }

    public static bool IsCompleted(TaskId id)
    {
        return Instance != null
            && Instance._states.TryGetValue(id, out var s)
            && s == TaskState.Completed;
    }
}
