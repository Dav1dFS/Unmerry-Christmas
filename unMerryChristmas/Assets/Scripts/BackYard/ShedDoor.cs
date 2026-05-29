using UnityEngine;
public class ShedDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _doorPivot;
    [SerializeField] private float     _openAngle = 110f;
    [SerializeField] private float     _openSpeed = 3f;

    private bool _unlocked;
    private bool _open;
    private Quaternion _targetRotation;

    private void Awake()
    {
        _targetRotation = Quaternion.identity;
    }

    public void Unlock()
    {
        _unlocked = true;
    }

    public void Interact()
    {
        if (!_unlocked) return;
        if (_open) return;

        _open = true;
        _targetRotation = Quaternion.Euler(0f, _openAngle, 0f);
    }

    public string GetHintText() => _unlocked ? "E — Open Shed" : "E — (Locked)";

    private void Update()
    {
        if (!_open) return;
        _doorPivot.localRotation = Quaternion.Slerp(
            _doorPivot.localRotation,
            _targetRotation,
            _openSpeed * Time.deltaTime
        );
    }
}
