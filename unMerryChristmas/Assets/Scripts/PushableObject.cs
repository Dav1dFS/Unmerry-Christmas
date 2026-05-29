using System;
using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float _pushForce   = 800f;
    [SerializeField] private float _linearDrag  = 8f;
    [SerializeField] private float _angularDrag = 8f;
    [SerializeField] private bool  _lockRotation = false;

    public event Action OnReleased;

    private Rigidbody rb;
    private bool _grabbed;

    private const RigidbodyConstraints IdleConstraints =
        RigidbodyConstraints.FreezePositionX |
        RigidbodyConstraints.FreezePositionZ |
        RigidbodyConstraints.FreezeRotation;

    // Grabbed with rotation allowed: XZ translation free, Y rotation free, Y position and XZ tipping frozen
    private const RigidbodyConstraints GrabbedConstraints =
        RigidbodyConstraints.FreezePositionY |
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationZ;

    // Grabbed with rotation locked: XZ translation free, all rotation frozen, Y position frozen
    private const RigidbodyConstraints GrabbedConstraintsLocked =
        RigidbodyConstraints.FreezePositionY |
        RigidbodyConstraints.FreezeRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = IdleConstraints;
    }

    void FixedUpdate()
    {
        // While idle, allow gravity but prevent upward collision deflection
        if (!_grabbed && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }

    public void OnGrab()
    {
        _grabbed          = true;
        rb.constraints    = _lockRotation ? GrabbedConstraintsLocked : GrabbedConstraints;
        rb.linearDamping  = _linearDrag;
        rb.angularDamping = _angularDrag;
    }

    public void OnRelease()
    {
        _grabbed           = false;
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints     = IdleConstraints;
        OnReleased?.Invoke();
    }

    public void ApplyPushForce(Vector3 direction, Vector3 worldContactPoint)
    {
        Vector3 horizontal = new Vector3(direction.x, 0f, direction.z);
        rb.AddForceAtPosition(horizontal * _pushForce, worldContactPoint);
    }
}
