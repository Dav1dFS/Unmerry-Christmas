using System;
using UnityEngine;

[System.Flags]
public enum PushAxis { None = 0, X = 1, Z = 2, Both = X | Z }

public class PushableObject : MonoBehaviour
{
    [SerializeField] public PushAxis allowedAxes = PushAxis.Both;
    [SerializeField] public float maxPushDistance = 5f;
    [SerializeField] private float _pushForce = 800f;
    [SerializeField] private float _linearDrag = 8f;
    [SerializeField] private float _angularDrag = 8f;

    public event Action OnDisplaced;

    private Rigidbody rb;
    private Vector3 _startPosition;
    private bool _displacedFired;

    // Idle: fully frozen so walking into it does nothing
    private const RigidbodyConstraints IdleConstraints =
        RigidbodyConstraints.FreezePositionX |
        RigidbodyConstraints.FreezePositionZ |
        RigidbodyConstraints.FreezeRotation;

    // Grabbed: allow XZ translation and Y rotation (natural spinning), freeze Y position and XZ tipping
    private const RigidbodyConstraints GrabbedConstraints =
        RigidbodyConstraints.FreezePositionY |
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationZ;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = IdleConstraints;
    }

    public void OnGrab()
    {
        _startPosition = rb.position;
        _displacedFired = false;
        rb.constraints = GrabbedConstraints;
        rb.linearDamping = _linearDrag;
        rb.angularDamping = _angularDrag;
    }

    public void OnRelease()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = IdleConstraints;
    }

    public bool IsWithinPushDistance()
    {
        Vector2 displacement = new Vector2(
            rb.position.x - _startPosition.x,
            rb.position.z - _startPosition.z
        );
        bool within = displacement.magnitude < maxPushDistance;
        if (!within && !_displacedFired)
        {
            _displacedFired = true;
            OnDisplaced?.Invoke();
        }
        return within;
    }

    // Applies force at a specific point on the surface, creating natural torque when off-center
    public void ApplyPushForce(Vector3 direction, Vector3 worldContactPoint)
    {
        if (!IsWithinPushDistance()) return;
        Vector3 filtered = new Vector3(
            (allowedAxes & PushAxis.X) != 0 ? direction.x : 0f,
            0f,
            (allowedAxes & PushAxis.Z) != 0 ? direction.z : 0f
        );
        rb.AddForceAtPosition(filtered * _pushForce, worldContactPoint);
    }
}
