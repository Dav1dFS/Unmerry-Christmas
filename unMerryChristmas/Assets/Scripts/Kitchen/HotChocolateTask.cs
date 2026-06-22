using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Task 2 coordinator. Watches every <see cref="DrinkableHotChocolate"/> in the scene.
/// When all of them have been drunk it completes the linked task, which reveals the
/// ExplosivePresents ability token via that token's <see cref="TaskRewardReveal"/>.
/// </summary>
public class HotChocolateTask : MonoBehaviour
{
    [SerializeField] private TaskDefinition _task;

    [Tooltip("Leave empty to auto-collect every DrinkableHotChocolate in the scene on Awake.")]
    [SerializeField] private List<DrinkableHotChocolate> _drinks = new();

    private int _remaining;
    private bool _completed;

    private void Awake()
    {
        if (_drinks.Count == 0)
            _drinks.AddRange(FindObjectsByType<DrinkableHotChocolate>(FindObjectsSortMode.None));

        foreach (var drink in _drinks)
        {
            if (drink == null) continue;
            _remaining++;
            drink.OnDrunk += HandleDrunk;
        }
    }

    private void OnDestroy()
    {
        foreach (var drink in _drinks)
            if (drink != null) drink.OnDrunk -= HandleDrunk;
    }

    private void HandleDrunk()
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
