using UnityEngine;
using System.Collections;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;

    // The camera cycles through these offsets in order each time R is pressed.
    // Tune each entry independently in the Inspector.
    // The original back-left view, rotated 90° three times so all four are evenly
    // spaced around the target while keeping the original's angle, height and distance.
    [SerializeField] private Vector3[] _offsets =
    {
        new Vector3(-7.5f, 5f, -6f), // back-left (original view)
        new Vector3(-6f, 5f, 7.5f),  // front-left
        new Vector3(7.5f, 5f, 6f),   // front-right
        new Vector3(6f, 5f, -7.5f),  // back-right
    };

    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _fieldOfView = 50f;

    private Vector3 _activeTargetOffset; // The offset we want to reach
    private Vector3 _smoothedOffset;     // The intermediate offset currently being calculated
    private Vector3 _offsetVelocity = Vector3.zero;
    private Vector3 _shakeOffset = Vector3.zero;

    private Camera _cam;
    private int _offsetIndex; // Index into _offsets of the current view

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null)
            _cam.fieldOfView = _fieldOfView;

        // Initialize to the first configured offset
        Vector3 startOffset = (_offsets != null && _offsets.Length > 0) ? _offsets[0] : Vector3.zero;
        _activeTargetOffset = startOffset;
        _smoothedOffset = startOffset;

        LogActiveOffset();
    }

    private void LogActiveOffset()
    {
        if (_offsets == null || _offsets.Length == 0) return;
        Debug.Log($"[Camera] View {_offsetIndex + 1}/{_offsets.Length} -> offset {_offsets[_offsetIndex]}", this);
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

    /// <summary>
    /// Advances to the next configured camera offset, wrapping back to the first
    /// after the last one.
    /// </summary>
    public void CycleView()
    {
        if (_offsets == null || _offsets.Length == 0) return;

        _offsetIndex = (_offsetIndex + 1) % _offsets.Length;
        _activeTargetOffset = _offsets[_offsetIndex];

        // Reset tracking velocity to ensure a clean start to the transition
        _offsetVelocity = Vector3.zero;

        LogActiveOffset();
    }

    // Called by CameraShake
    public void SetShakeOffset(Vector3 offset) => _shakeOffset = offset;
}
