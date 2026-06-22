using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Task 1. A trigger volume on the trash can. When both nutcrackers are thrown in,
/// it completes the linked task, which reveals the hidden drawing page via that
/// page's <see cref="TaskRewardReveal"/>.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrashReceptacle : MonoBehaviour
{
    [SerializeField] private TaskDefinition _task;
    [SerializeField] private int _requiredCount = 2;

    [Tooltip("Destroy each nutcracker as it lands in the trash.")]
    [SerializeField] private bool _consume = true;

    private readonly HashSet<Nutcracker> _counted = new();
    private bool _completed;

    private void OnTriggerEnter(Collider other)
    {
        if (_completed) return;

        var nut = other.GetComponentInParent<Nutcracker>();
        if (nut == null) return;
        if (!_counted.Add(nut)) return; // already counted (re-entry / multiple colliders)

        if (_consume) Destroy(nut.gameObject);

        if (_counted.Count >= _requiredCount)
        {
            _completed = true;
            TaskService.ReportTask(_task);
        }
    }
}
