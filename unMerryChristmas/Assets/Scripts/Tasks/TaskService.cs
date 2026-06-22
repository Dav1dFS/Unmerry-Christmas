using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-agnostic runtime task store. Mirrors the public surface of
/// <c>BackYardTaskTracker</c> (ReportTask / UnlockTask / IsCompleted / IsAvailable /
/// OnTaskCompleted) but is keyed by <see cref="TaskDefinition"/> assets instead of a
/// fixed enum, so any scene can own its own tasks.
///
/// The Backyard intentionally stays on its own tracker (it's done and working); this
/// service is used by the Kitchen and any future scene. A single instance persists
/// across scenes (DontDestroyOnLoad). Each scene's <see cref="SceneTaskRegistrar"/>
/// registers its <see cref="TaskSet"/> under its scene name; UI looks up the set for
/// the active scene, so the two systems never collide.
///
/// State is in-memory only, matching the rest of the game's managers. When a save
/// system is added, serialise the per-definition states here (definitions have stable
/// asset GUIDs).
/// </summary>
public class TaskService : MonoBehaviour
{
    public static TaskService Instance { get; private set; }

    /// <summary>Fires when a task transitions Available → Completed.</summary>
    public static event Action<TaskDefinition> OnTaskCompleted;

    private readonly Dictionary<TaskDefinition, TaskState> _states = new();
    private readonly Dictionary<string, TaskSet> _setsByScene = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        AbilityTokenManager.OnAbilityUnlocked += HandleAbilityUnlocked;
    }

    private void OnDestroy()
    {
        AbilityTokenManager.OnAbilityUnlocked -= HandleAbilityUnlocked;
        if (Instance == this) Instance = null;
    }

    /// <summary>Create the singleton on demand. Safe to call from any scene's Awake.</summary>
    public static TaskService EnsureExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("TaskService");
            Instance = go.AddComponent<TaskService>();
        }
        return Instance;
    }

    /// <summary>
    /// Register a scene's task set: seed any unseen definitions with their initial
    /// state (Locked if locked-by-default, else Available) and record the set under
    /// the given scene name for UI lookup. Idempotent — existing states are kept so a
    /// re-visit doesn't reset progress.
    /// </summary>
    public static void Register(TaskSet set, string sceneName)
    {
        if (set == null) return;
        var svc = EnsureExists();
        if (!string.IsNullOrEmpty(sceneName))
            svc._setsByScene[sceneName] = set;

        foreach (var def in set.Tasks)
        {
            if (def == null || svc._states.ContainsKey(def)) continue;

            var state = def.LockedByDefault ? TaskState.Locked : TaskState.Available;

            // If the unlocking ability was already granted before this scene
            // registered, start Available rather than waiting for an event we missed.
            if (state == TaskState.Locked
                && AbilityTokenManager.Instance != null
                && AbilityTokenManager.Instance.IsUnlocked(def.UnlockAbility))
                state = TaskState.Available;

            svc._states[def] = state;
        }
    }

    /// <summary>The task set registered for the given scene, or null if none.</summary>
    public static TaskSet GetSetForScene(string sceneName)
    {
        if (Instance != null && sceneName != null
            && Instance._setsByScene.TryGetValue(sceneName, out var set))
            return set;
        return null;
    }

    private void HandleAbilityUnlocked(PlayerAbility ability)
    {
        // Copy keys first — we mutate _states inside the loop.
        var keys = new List<TaskDefinition>(_states.Keys);
        foreach (var def in keys)
        {
            if (_states[def] == TaskState.Locked
                && def.LockedByDefault
                && def.UnlockAbility == ability)
            {
                _states[def] = TaskState.Available;
            }
        }
    }

    public static void UnlockTask(TaskDefinition def)
    {
        if (Instance == null || def == null) return;
        if (Instance._states.TryGetValue(def, out var s) && s == TaskState.Locked)
            Instance._states[def] = TaskState.Available;
    }

    public static void ReportTask(TaskDefinition def)
    {
        if (Instance == null || def == null) return;
        if (!Instance._states.TryGetValue(def, out var s) || s != TaskState.Available) return;

        Instance._states[def] = TaskState.Completed;
        OnTaskCompleted?.Invoke(def);
    }

    public static bool IsCompleted(TaskDefinition def)
        => Instance != null && def != null
           && Instance._states.TryGetValue(def, out var s) && s == TaskState.Completed;

    public static bool IsAvailable(TaskDefinition def)
        => Instance != null && def != null
           && Instance._states.TryGetValue(def, out var s) && s == TaskState.Available;
}
