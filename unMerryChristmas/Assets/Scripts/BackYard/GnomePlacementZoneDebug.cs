using UnityEngine;

/// <summary>
/// Debug version of GnomePlacementZone with detailed logging
/// to troubleshoot gnome placement issues.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GnomePlacementZoneDebug : MonoBehaviour
{
    public System.Action OnGnomePlaced;
    public bool IsFilled { get; private set; }

    private GnomePlacementZone _originalZone;
    private string _zoneName;

    private void Start()
    {
        _zoneName = gameObject.name;
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[{_zoneName}] Collider is NOT set as trigger! Setting it now...");
            col.isTrigger = true;
        }
        else
        {
            Debug.Log($"[{_zoneName}] Collider is properly set as trigger ✓");
        }

        Debug.Log($"[{_zoneName}] Zone initialized. Waiting for gnome placement...");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{_zoneName}] OnTriggerEnter: {other.gameObject.name}");
        CheckGnomeAndPlace(other, "OnTriggerEnter");
    }

    private void OnTriggerStay(Collider other)
    {
        // Only check if not already filled
        if (!IsFilled)
        {
            CheckGnomeAndPlace(other, "OnTriggerStay");
        }
    }

    private void CheckGnomeAndPlace(Collider other, string triggerType)
    {
        if (IsFilled)
        {
            Debug.Log($"[{_zoneName}] Already filled, ignoring {other.gameObject.name}");
            return;
        }

        // Check tag
        if (!other.CompareTag("Gnome"))
        {
            Debug.Log($"[{_zoneName}] {triggerType}: {other.gameObject.name} - NOT a gnome (tag: {other.gameObject.tag})");
            return;
        }

        Debug.Log($"[{_zoneName}] {triggerType}: {other.gameObject.name} is a GNOME! Checking rigidbody...");

        // Get rigidbody
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.LogWarning($"[{_zoneName}] Gnome {other.gameObject.name} has NO rigidbody!");
            return;
        }

        Debug.Log($"[{_zoneName}] Gnome {other.gameObject.name} rigidbody state: isKinematic={rb.isKinematic}");

        // Check if held (kinematic = being held)
        if (rb.isKinematic)
        {
            Debug.Log($"[{_zoneName}] Gnome {other.gameObject.name} is HELD (kinematic). Need to drop it.");
            return;
        }

        // SUCCESS - Gnome is in zone and dropped!
        Debug.Log($"[{_zoneName}] ✓ GNOME PLACED! {other.gameObject.name} registered in zone!");
        IsFilled = true;
        OnGnomePlaced?.Invoke();
    }
}
