using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GnomePlacementZoneDebug : MonoBehaviour
{
    public System.Action OnGnomePlaced;
    public bool IsFilled { get; private set; }

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckAndPlace(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsFilled)
            CheckAndPlace(other);
    }

    private void CheckAndPlace(Collider other)
    {
        if (IsFilled) return;
        if (!other.CompareTag("Gnome")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        if (rb.isKinematic) return;

        PlaceGnome(other.gameObject);
    }

    private void PlaceGnome(GameObject gnome)
    {
        IsFilled = true;

        var bounds = GetComponent<Collider>().bounds;

        var gnomeCollider = gnome.GetComponent<Collider>();
        float gnomeHeight = gnomeCollider != null ? gnomeCollider.bounds.size.y : 0f;

        gnome.transform.position = new Vector3(bounds.center.x, bounds.max.y + gnomeHeight / 2, bounds.center.z);
        gnome.transform.rotation = Quaternion.Euler(-90, 180, 0);

        var rb = gnome.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic      = true;
            rb.linearVelocity   = Vector3.zero;
            rb.angularVelocity  = Vector3.zero;
        }

        OnGnomePlaced?.Invoke();
    }
}
