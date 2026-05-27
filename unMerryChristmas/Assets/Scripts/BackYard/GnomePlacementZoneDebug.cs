using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GnomePlacementZoneDebug : MonoBehaviour
{
    public System.Action OnGnomePlaced;
    public bool IsFilled { get; private set; }
    private string _zoneName;

    private void Awake()
    {
        _zoneName = gameObject.name;
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        Debug.Log($"[{_zoneName}] Zone initialized with trigger collider");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{_zoneName}] OnTriggerEnter: {other.gameObject.name} (tag: {other.gameObject.tag})");
        CheckAndPlace(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsFilled)
        {
            Debug.Log($"[{_zoneName}] OnTriggerStay: {other.gameObject.name}");
            CheckAndPlace(other);
        }
    }

    private void CheckAndPlace(Collider other)
    {
        if (IsFilled)
        {
            Debug.Log($"[{_zoneName}] Already filled");
            return;
        }

        if (!other.CompareTag("Gnome"))
        {
            Debug.Log($"[{_zoneName}] {other.gameObject.name} is not a Gnome (tag: {other.gameObject.tag})");
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        Debug.Log($"[{_zoneName}] Gnome rigidbody: {(rb != null ? rb.name : "NULL")}");

        if (rb == null)
        {
            Debug.Log($"[{_zoneName}] No rigidbody found");
            return;
        }

        Debug.Log($"[{_zoneName}] Rigidbody isKinematic: {rb.isKinematic}");
        if (rb.isKinematic)
        {
            Debug.Log($"[{_zoneName}] Gnome is kinematic (held), not placing");
            return;
        }

        Debug.Log($"[{_zoneName}] ✓ PLACING GNOME: {other.gameObject.name}");
        PlaceGnome(other.gameObject);
    }

    private void PlaceGnome(GameObject gnome)
    {
        IsFilled = true;

        var zoneCollider = GetComponent<Collider>();
        var bounds = zoneCollider.bounds;

        var gnomeCollider = gnome.GetComponent<Collider>();
        float gnomeHeight = 0;
        if (gnomeCollider != null)
        {
            gnomeHeight = gnomeCollider.bounds.size.y;
        }

        Vector3 newPos = new Vector3(bounds.center.x, bounds.max.y + gnomeHeight / 2, bounds.center.z);
        gnome.transform.position = newPos;
        gnome.transform.rotation = Quaternion.Euler(-90, 180, 0);

        var rb = gnome.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"[{_zoneName}] Gnome placed at {newPos}");
        OnGnomePlaced?.Invoke();
    }
}
