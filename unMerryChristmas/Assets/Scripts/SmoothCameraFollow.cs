using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _defaultOffset = new Vector3(0f, 1.5f, -1f);
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _fieldOfView = 50f;

    private Vector3 _velocity = Vector3.zero;
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null)
            _cam.fieldOfView = _fieldOfView;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = _target.position + _defaultOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
        transform.LookAt(_target);
    }
}
