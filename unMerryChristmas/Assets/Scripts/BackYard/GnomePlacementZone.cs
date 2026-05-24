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

        PlaceGnome(other.gameObject);
    }

    // Fallback: re-check every fixed frame in case the gnome lands after the trigger fires
    private void OnTriggerStay(Collider other)
    {
        if (IsFilled) return;
        if (!other.CompareTag("Gnome")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        PlaceGnome(other.gameObject);
    }

    private void PlaceGnome(GameObject gnome)
    {
        IsFilled = true;

        // Position gnome on top of the zone
        var zoneCollider = GetComponent<Collider>();
        var bounds = zoneCollider.bounds;

        // Get gnome's height
        var gnomeCollider = gnome.GetComponent<Collider>();
        float gnomeHeight = 0;
        if (gnomeCollider != null)
        {
            gnomeHeight = gnomeCollider.bounds.size.y;
        }

        // Position gnome center at zone top
        Vector3 newPos = new Vector3(bounds.center.x, bounds.max.y + gnomeHeight / 2, bounds.center.z);
        gnome.transform.position = newPos;

        // Apply -90 on X axis and 180 on Y axis
        gnome.transform.rotation = Quaternion.Euler(-90, 180, 0);

        // Freeze gnome in place
        var rb = gnome.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        OnGnomePlaced?.Invoke();
    }
}
