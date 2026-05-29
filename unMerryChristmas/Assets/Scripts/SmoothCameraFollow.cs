using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _defaultOffset = new Vector3(-7.5f, 5f, -6f); // Matching your Inspector image
    [SerializeField] private Vector3 _alternateOffset = new Vector3(0f, 8f, -10f); // Set your second view position here
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _fieldOfView = 50f;

    private Vector3 _currentOffset;
    private Vector3 _velocity = Vector3.zero;
    private Camera _cam;
    private bool _usingDefaultOffset = true;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null)
            _cam.fieldOfView = _fieldOfView;

        // Start with the default offset config
        _currentOffset = _defaultOffset;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // SmoothDamp will inherently create a clean interpolation when _currentOffset changes!
        Vector3 targetPosition = _target.position + _currentOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
        transform.LookAt(_target);
    }

    /// <summary>
    /// Toggles between the two designated camera offset points.
    /// </summary>
    public void ToggleOffset()
    {
        _usingDefaultOffset = !_usingDefaultOffset;
        _currentOffset = _usingDefaultOffset ? _defaultOffset : _alternateOffset;
    }
}