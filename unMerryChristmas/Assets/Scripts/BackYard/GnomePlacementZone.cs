using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GnomePlacementZone : MonoBehaviour
{
    public event Action OnGnomePlaced;
    public bool IsFilled { get; private set; }

    // Derived at runtime from this zone's own name (e.g. "GnomePlacementZone_2" → 2).
    // Never serialized, so stale Inspector values can't interfere.
    private int gnomeIndex = 0;

    private static readonly Vector3[] HardcodedPositions = {
        new Vector3(5.853f,  0.09834625f, -15.920486f),
        new Vector3(5.848f,  0.09834611f, -15.34f),
        new Vector3(5.856f,  0.09834640f, -14.4f),
    };
    private static readonly Quaternion[] HardcodedRotations = {
        new Quaternion(-0.5f, -0.5f, -0.5f,  0.5f),
        new Quaternion( 0.5f,  0.5f,  0.5f, -0.5f),
        new Quaternion(-0.5f, -0.5f, -0.5f,  0.5f),
    };
    private static readonly Vector3 HardcodedScale = new Vector3(11.343484f, 11.343477f, 11.343477f);

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        // Always derive index from zone name — never rely on serialized value
        string[] parts = gameObject.name.Split('_');
        if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int parsed))
        {
            gnomeIndex = parsed;
            Debug.Log($"[GnomePlacementZone] '{gameObject.name}' → gnomeIndex={gnomeIndex}");
        }
        else
            Debug.LogError($"[GnomePlacementZone] Cannot parse gnome index from name '{gameObject.name}'. Expected format: GnomePlacementZone_N", this);
    }

    private bool IsAcceptedGnome(GameObject gnome)
    {
        string expectedName = "Gnome_" + gnomeIndex + "_";
        string exactName    = "Gnome_" + gnomeIndex;
        bool accepted = gnome.name == exactName || gnome.name.StartsWith(expectedName);

        Debug.Log($"[GnomePlacementZone] Zone '{gameObject.name}' (index={gnomeIndex}) vs gnome '{gnome.name}' → {(accepted ? "ACCEPTED" : "REJECTED")}");
        return accepted;
    }

    private void OnTriggerEnter(Collider other) => TryPlace(other);
    private void OnTriggerStay(Collider other)  => TryPlace(other);

    private void TryPlace(Collider other)
    {
        if (IsFilled) return;
        if (!other.CompareTag("Gnome")) return;

        PickupObject pickup = other.GetComponentInParent<PickupObject>();
        if (pickup == null || !pickup.HasBeenPickedUp) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        Debug.Log($"[GnomePlacementZone] TryPlace: zone='{gameObject.name}' other='{other.gameObject.name}' rb='{rb.gameObject.name}'");
        if (!IsAcceptedGnome(rb.gameObject)) return;

        PlaceGnome(rb.gameObject, rb);
    }

    private void PlaceGnome(GameObject gnome, Rigidbody rb)
    {
        IsFilled = true;

        // Freeze physics before touching the transform
        rb.isKinematic = true;
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply hardcoded world transform
        int i = Mathf.Clamp(gnomeIndex - 1, 0, 2);
        gnome.transform.SetPositionAndRotation(HardcodedPositions[i], HardcodedRotations[i]);
        gnome.transform.localScale = HardcodedScale;

        // Move to Default layer and remove Gnome tag
        gnome.tag   = "Untagged";
        gnome.layer = LayerMask.NameToLayer("Default");

        // Disable all PickupObject components so it can't be grabbed again
        foreach (var p in gnome.GetComponentsInChildren<PickupObject>(true))
            p.enabled = false;

        OnGnomePlaced?.Invoke();
    }
}
