using UnityEngine;

/// <summary>
/// Keeps the camera from passing through walls in enclosed scenarios (e.g. the kitchen).
///
/// This is a self-contained add-on. It pulls the camera in along the line to the target
/// whenever a wall sits in the way. The shared SmoothCameraFollow script is left completely
/// untouched, so other scenarios behave exactly as before — only cameras that actually have
/// this component attached get wall avoidance.
///
/// The pull-in runs in Camera.onPreCull, which fires AFTER every script's LateUpdate (so the
/// follow camera has already placed itself) but BEFORE the camera renders. That removes any
/// dependence on script execution order — the follow camera can never overwrite the result.
///
/// Requires the room/walls to have a Collider. Assign the room to <see cref="_obstacle"/> so only
/// it blocks the camera (props with colliders are then ignored).
/// </summary>
public class CameraWallCollision : MonoBehaviour
{
    [Tooltip("What the camera looks at / pivots around. Assign the same target the follow camera uses (the player).")]
    [SerializeField] private Transform _target;

    [Tooltip("If set, ONLY colliders on this object (and its children) block the camera — everything else (props, decorations) is ignored. " +
             "Assign the kitchen room here. Leave empty to collide with everything on the mask except the player.")]
    [SerializeField] private Transform _obstacle;

    [Tooltip("Layers treated as walls/obstacles. The target's own hierarchy is always ignored, so it is safe to leave this as Everything.")]
    [SerializeField] private LayerMask _collisionMask = ~0;

    [Tooltip("Camera 'thickness' kept off surfaces. Larger = camera stops further from walls.")]
    [SerializeField] private float _collisionRadius = 0.3f;

    [Tooltip("The camera never gets closer to the target than this, even when pinned against a wall.")]
    [SerializeField] private float _minDistance = 1.2f;

    [Tooltip("How quickly the camera eases back out once the wall is no longer in the way (seconds).")]
    [SerializeField] private float _easeOutTime = 0.25f;

    [Tooltip("Draw the collision cast in the Scene view and log hits + final distance, to diagnose. Turn off once it works.")]
    [SerializeField] private bool _debugDraw = true;

    private float _currentDistance = -1f; // smoothed distance after collision (<0 = uninitialized)
    private float _distanceVelocity;      // SmoothDamp state for easing back out
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[32];
    private Camera _cam;

    private void Awake() => _cam = GetComponent<Camera>();

    private void OnEnable()  => Camera.onPreCull += ApplyCollision;
    private void OnDisable() => Camera.onPreCull -= ApplyCollision;

    // Runs after all LateUpdates, just before this camera renders.
    private void ApplyCollision(Camera cam)
    {
        if (cam != _cam || _target == null) return;

        Vector3 pivot = _target.position;
        Vector3 toCamera = transform.position - pivot; // position already set by the follow camera this frame
        float fullDistance = toCamera.magnitude;
        if (fullDistance < 0.001f) return;

        Vector3 dir = toCamera / fullDistance;
        LayerMask mask = (_collisionMask == 0) ? (LayerMask)(~0) : _collisionMask; // 0 would hit nothing

        float desiredDistance = fullDistance;
        int hits = Physics.SphereCastNonAlloc(
            pivot, _collisionRadius, dir, _hitBuffer,
            fullDistance, mask, QueryTriggerInteraction.Ignore);

        float nearest = fullDistance;
        Collider hitCollider = null;
        for (int i = 0; i < hits; i++)
        {
            RaycastHit hit = _hitBuffer[i];
            if (hit.distance <= 0f) continue;                          // started overlapping a collider
            if (hit.collider.transform.root == _target.root) continue; // ignore the player itself
            if (_obstacle != null && !hit.collider.transform.IsChildOf(_obstacle)) continue; // only the kitchen blocks the camera
            if (hit.distance < nearest) { nearest = hit.distance; hitCollider = hit.collider; }
        }
        if (nearest < fullDistance) desiredDistance = Mathf.Max(nearest, _minDistance);

        if (_currentDistance < 0f) _currentDistance = desiredDistance; // first frame
        // Snap inward instantly (never reveal the outside even for one frame), ease back out smoothly.
        if (desiredDistance < _currentDistance)
            _currentDistance = desiredDistance;
        else
            _currentDistance = Mathf.SmoothDamp(
                _currentDistance, desiredDistance, ref _distanceVelocity, _easeOutTime);

        transform.position = pivot + dir * _currentDistance;
        transform.LookAt(_target);

        if (_debugDraw)
        {
            Color c = hitCollider != null ? Color.red : Color.green;
            Debug.DrawLine(pivot, transform.position, c);
            Debug.Log($"[CameraWallCollision] {(hitCollider != null ? "HIT '" + hitCollider.name + "'" : "clear")} | " +
                      $"applied camera distance = {_currentDistance:F2}m (full would be {fullDistance:F2}m)");
        }
    }
}
