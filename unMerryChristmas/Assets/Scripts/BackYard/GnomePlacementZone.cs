using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GnomePlacementZone : MonoBehaviour
{
    public event Action OnGnomePlaced;
    public bool IsFilled { get; private set; }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsFilled) return;
        if (!other.CompareTag("Gnome")) return;

        Rigidbody rb = other.attachedRigidbody;
        // Only count when the gnome is dropped (non-kinematic = not currently held)
        if (rb == null || rb.isKinematic) return;

        IsFilled = true;
        OnGnomePlaced?.Invoke();
    }

    // Fallback: re-check every fixed frame in case the gnome lands after the trigger fires
    private void OnTriggerStay(Collider other)
    {
        if (IsFilled) return;
        if (!other.CompareTag("Gnome")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        IsFilled = true;
        OnGnomePlaced?.Invoke();
    }
}
