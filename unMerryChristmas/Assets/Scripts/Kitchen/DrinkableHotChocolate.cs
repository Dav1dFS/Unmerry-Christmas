using System;
using UnityEngine;

/// <summary>
/// Task 2. An interactable mug of hot chocolate. Interacting "drinks" it: the full
/// mug is swapped for the empty <c>Mug_Christmas</c> prefab and listeners
/// (<see cref="HotChocolateTask"/>) are notified.
///
/// Detected by the player's interaction sweep, which runs on the Pickable layer and
/// calls <see cref="IInteractable.Interact"/> on the closest non-pickup. So this object
/// must sit on the Pickable layer with a (non-trigger) collider and must NOT carry a
/// PickupObject or a Collectable/Token tag — the Setup Kitchen Tasks tool handles that.
/// </summary>
public class DrinkableHotChocolate : MonoBehaviour, IInteractable
{
    [Tooltip("The empty mug spawned in place when this hot chocolate is drunk.")]
    [SerializeField] private GameObject _emptyMugPrefab;

    /// <summary>Fires once, when this hot chocolate is drunk.</summary>
    public event Action OnDrunk;

    private bool _drunk;

    public void Interact()
    {
        if (_drunk) return;
        _drunk = true;
    
        // Leave the empty mug behind so the kitchen visibly fills up with finished drinks.
        if (_emptyMugPrefab != null)
            Instantiate(_emptyMugPrefab, transform.position, transform.rotation, transform.parent);

        OnDrunk?.Invoke();
        Destroy(gameObject);
    }

    public string GetHintText() => "E — Drink";
}
