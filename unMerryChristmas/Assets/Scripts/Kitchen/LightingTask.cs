using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Task 3 coordinator. Completes the linked task once every wired
/// <see cref="LightableEmissive"/> (the wreath and the candle) is lit. Grants no
/// collectible — no <see cref="TaskRewardReveal"/> points at this task; the reward is
/// simply the lit decorations.
///
/// The task is Locked until the ExplosivePresents ability is unlocked (set on the
/// TaskDefinition), which is also what makes lighting possible in the first place.
/// </summary>
public class LightingTask : MonoBehaviour
{
    [SerializeField] private TaskDefinition _task;

    [Tooltip("Leave empty to auto-collect every LightableEmissive in the scene on Awake.")]
    [SerializeField] private List<LightableEmissive> _lightables = new();

    private int _remaining;
    private bool _completed;

    private void Awake()
    {
        if (_lightables.Count == 0)
            _lightables.AddRange(FindObjectsByType<LightableEmissive>(FindObjectsSortMode.None));

        foreach (var lightable in _lightables)
        {
            if (lightable == null || lightable.IsLit) continue;
            _remaining++;
            lightable.OnLit += HandleLit;
        }
    }

    private void OnDestroy()
    {
        foreach (var lightable in _lightables)
            if (lightable != null) lightable.OnLit -= HandleLit;
    }

    private void HandleLit()
    {
        if (_completed) return;
        _remaining--;
        if (_remaining <= 0)
        {
            _completed = true;
            TaskService.ReportTask(_task);
        }
    }
}
