using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _defaultOffset = new Vector3(-7.5f, 5f, -6f); 
    [SerializeField] private Vector3 _alternateOffset = new Vector3(0f, 8f, -10f); 
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _fieldOfView = 50f;

    private Vector3 _activeTargetOffset; // The offset we want to reach
    private Vector3 _smoothedOffset;     // The intermediate offset currently being calculated
    private Vector3 _offsetVelocity = Vector3.zero; 
    
    private Camera _cam;
    private bool _usingDefaultOffset = true;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null)
            _cam.fieldOfView = _fieldOfView;

        // Initialize both offsets to the default view
        _activeTargetOffset = _defaultOffset;
        _smoothedOffset = _defaultOffset;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // 1. Smoothly interpolate the OFFSET itself, not the world position
        _smoothedOffset = Vector3.SmoothDamp(_smoothedOffset, _activeTargetOffset, ref _offsetVelocity, _smoothTime);
        
        // 2. Apply the smoothed offset directly to the target's current position
        transform.position = _target.position + _smoothedOffset;
        
        // 3. Keep the target locked perfectly dead-center of the screen
        transform.LookAt(_target);
    }

    /// <summary>
    /// Toggles between the two designated camera offset points smoothly.
    /// </summary>
    public void ToggleOffset()
    {
        _usingDefaultOffset = !_usingDefaultOffset;
        _activeTargetOffset = _usingDefaultOffset ? _defaultOffset : _alternateOffset;

        // Reset tracking velocity to ensure a clean start to the transition
        _offsetVelocity = Vector3.zero; 
    }
}