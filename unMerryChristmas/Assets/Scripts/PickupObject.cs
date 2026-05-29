using System;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public event Action OnPickedUp;
    public event Action OnDropped;

    [SerializeField] private Vector3 holdScale = Vector3.one;
    [SerializeField] private Vector3 holdRotationOffset = Vector3.zero;

    public bool HasBeenPickedUp { get; private set; }
    public Quaternion NaturalWorldRotation { get; private set; }
    public Vector3 NaturalWorldScale { get; private set; }

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalLocalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalLocalScale = transform.localScale;
        // Record world-space values while still parented correctly in the scene
        NaturalWorldRotation = transform.rotation;
        NaturalWorldScale    = transform.lossyScale;
    }

    public void OnPickup(Transform holdPoint)
    {
        HasBeenPickedUp = true;
        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(holdRotationOffset);

        // Apply hold scale: explicit override OR preserve natural world size
        if (holdScale != Vector3.one)
        {
            transform.localScale = holdScale;
        }
        else
        {
            // Maintain natural world scale relative to the holdPoint parent
            Vector3 ps = holdPoint.lossyScale;
            transform.localScale = new Vector3(
                NaturalWorldScale.x / ps.x,
                NaturalWorldScale.y / ps.y,
                NaturalWorldScale.z / ps.z);
        }

        OnPickedUp?.Invoke();
    }

    public void OnDrop()
    {
        transform.SetParent(null);
        transform.localScale = originalLocalScale;

        rb.isKinematic = false;
        col.enabled = true;

        OnDropped?.Invoke();
    }
}
