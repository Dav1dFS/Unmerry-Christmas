using System;
using UnityEngine;

[RequireComponent(typeof(PickupObject))]
public class TopplableChair : MonoBehaviour
{
    public event Action OnToppled;

    private PickupObject _pickup;
    private bool _wasPickedUp;
    private bool _toppled;

    private void Awake() => _pickup = GetComponent<PickupObject>();

    private void OnEnable()
    {
        _pickup.OnPickedUp += () => _wasPickedUp = true;
        _pickup.OnDropped  += HandleDropped;
    }

    private void OnDisable() => _pickup.OnDropped -= HandleDropped;

    private void HandleDropped()
    {
        if (_toppled || !_wasPickedUp) return;
        _toppled = true;
        OnToppled?.Invoke();
    }
}
