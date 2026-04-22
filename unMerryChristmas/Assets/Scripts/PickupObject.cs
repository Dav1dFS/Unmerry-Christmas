using System;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public event Action OnPickedUp;
    public event Action OnDropped;

    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void OnPickup(Transform holdPoint)
    {
        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        OnPickedUp?.Invoke();
    }

    public void OnDrop()
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled = true;

        OnDropped?.Invoke();
    }
}
