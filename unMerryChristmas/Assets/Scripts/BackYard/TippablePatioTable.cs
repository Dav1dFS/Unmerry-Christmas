using UnityEngine;

[RequireComponent(typeof(PushableObject))]
[RequireComponent(typeof(Rigidbody))]
public class TippablePatioTable : MonoBehaviour
{
    [SerializeField] private float _tipTorque = 120f;
    [SerializeField] private GardenChaosTracker _gardenChaosTracker;

    private PushableObject _pushable;
    private Rigidbody _rb;
    private bool _tipped;

    private void Awake()
    {
        _pushable = GetComponent<PushableObject>();
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()  => _pushable.OnReleased += Tip;
    private void OnDisable() => _pushable.OnReleased -= Tip;

    private void Tip()
    {
        if (_tipped) return;
        _tipped = true;

        // Release all rotation constraints so physics can tip the table
        _rb.constraints = RigidbodyConstraints.None;

        // Push the centre of mass slightly off-axis so gravity finishes the job
        Vector3 torqueDir = Vector3.Cross(Vector3.up, transform.forward).normalized;
        _rb.AddTorque(torqueDir * _tipTorque, ForceMode.Impulse);

        _gardenChaosTracker?.OnTableTipped();
    }
}
