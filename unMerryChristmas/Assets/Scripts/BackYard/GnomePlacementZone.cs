using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GnomePlacementZone : MonoBehaviour
{
    public event Action OnGnomePlaced;
    public bool IsFilled { get; private set; }

    [SerializeField] private GameObject acceptedGnome;
    [SerializeField] private int gnomeIndex = 1; // 1, 2, or 3

    private static readonly Vector3[]    HardcodedPositions = {
        new Vector3(5.853f,  0.09834625f, -15.920486f),
        new Vector3(5.848f,  0.09834611f, -14.428000f),
        new Vector3(5.856f,  0.09834640f, -15.335675f),
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
    }

    private bool IsAcceptedGnome(GameObject gnome)
    {
        if (acceptedGnome == null) return true;
        return gnome == acceptedGnome || gnome.transform.IsChildOf(acceptedGnome.transform);
    }

    private void OnTriggerEnter(Collider other) => TryPlace(other);
    private void OnTriggerStay(Collider other)  => TryPlace(other);

    private void TryPlace(Collider other)
    {
        if (IsFilled) return;
        if (!other.CompareTag("Gnome")) return;

        PickupObject pickup = other.GetComponentInParent<PickupObject>();
        if (pickup == null || !pickup.HasBeenPickedUp) return;

        if (!IsAcceptedGnome(other.gameObject)) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        PlaceGnome(other.gameObject, rb);
    }

    private void PlaceGnome(GameObject gnome, Rigidbody rb)
    {
        IsFilled = true;

        // Freeze physics before touching the transform
        rb.isKinematic = true;
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply hardcoded world transform for this gnome index
        int i = Mathf.Clamp(gnomeIndex - 1, 0, 2);
        gnome.transform.SetPositionAndRotation(HardcodedPositions[i], HardcodedRotations[i]);
        gnome.transform.localScale = HardcodedScale;

        // Remove from pickup layer
        gnome.tag = "Untagged";

        // Disable all PickupObject components so it can't be grabbed again
        foreach (var p in gnome.GetComponentsInChildren<PickupObject>(true))
            p.enabled = false;

        OnGnomePlaced?.Invoke();
    }
}
