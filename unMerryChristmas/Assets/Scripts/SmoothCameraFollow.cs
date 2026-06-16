using UnityEngine;
using System.Collections;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _defaultOffset = new Vector3(-7.5f, 5f, -6f);
    [SerializeField] private Vector3 _alternateOffset = new Vector3(0f, 8f, -10f);
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _fieldOfView = 50f;

    private Vector3 _activeTargetOffset;
    private Vector3 _smoothedOffset;
    private Vector3 _offsetVelocity = Vector3.zero;
    private Vector3 _shakeOffset = Vector3.zero;   // added each frame on top

    private Camera _cam;
    private bool _usingDefaultOffset = true;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null)
            _cam.fieldOfView = _fieldOfView;

        _activeTargetOffset = _defaultOffset;
        _smoothedOffset = _defaultOffset;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        _smoothedOffset = Vector3.SmoothDamp(
            _smoothedOffset, _activeTargetOffset,
            ref _offsetVelocity, _smoothTime);

        // Shake offset is added on top of the follow position
        transform.position = _target.position + _smoothedOffset + _shakeOffset;
        transform.LookAt(_target);
    }

    public void ToggleOffset()
    {
        _usingDefaultOffset = !_usingDefaultOffset;
        _activeTargetOffset = _usingDefaultOffset ? _defaultOffset : _alternateOffset;
        _offsetVelocity = Vector3.zero;
    }

    // Called by CameraShake
    public void SetShakeOffset(Vector3 offset) => _shakeOffset = offset;
}