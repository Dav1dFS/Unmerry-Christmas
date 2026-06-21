using UnityEngine;

/// <summary>
/// Keeps the camera from passing through walls in enclosed scenarios (e.g. the kitchen).
///
/// This is a self-contained add-on: it runs AFTER the regular follow camera has placed
/// itself (see DefaultExecutionOrder) and simply pulls the camera in along the line to
/// the target when a wall sits in the way. The shared SmoothCameraFollow script is left
/// completely untouched, so other scenarios behave exactly as before — only the cameras
/// that actually have this component attached get wall avoidance.
///
/// Requires the room/walls to have a Collider on a layer included in <see cref="_collisionMask"/>.
/// </summary>
[DefaultExecutionOrder(100)] // run after follow cameras (default order 0) have set the position
public class CameraWallCollision : MonoBehaviour
{
    [Tooltip("What the camera looks at / pivots around. Assign the same target the follow camera uses (the player).")]
    [SerializeField] private Transform _target;

    [Tooltip("Layers treated as walls/obstacles. The target's own hierarchy is always ignored, so it is safe to leave this as Everything.")]
    [SerializeField] private LayerMask _collisionMask = ~0;

    [Tooltip("Camera 'thickness' kept off surfaces. Larger = camera stops further from walls.")]
    [SerializeField] private float _collisionRadius = 0.3f;

    [Tooltip("The camera never gets closer to the target than this, even when pinned against a wall.")]
    [SerializeField] private float _minDistance = 1.2f;

    [Tooltip("How quickly the camera eases back out once the wall is no longer in the way (seconds).")]
    [SerializeField] private float _easeOutTime = 0.25f;

    [Tooltip("Draw the collision cast in the Scene view and log hits, to diagnose whether walls are detected. Turn off once it works.")]
    [SerializeField] private bool _debugDraw = true;

    private float _currentDistance = -1f; // smoothed distance after collision (<0 = uninitialized)
    private float _distanceVelocity;      // SmoothDamp state for easing back out
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[8];

    private void LateUpdate()
    {
        if (_target == null) return;

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
            if (hit.distance < nearest) { nearest = hit.distance; hitCollider = hit.collider; }
        }
        if (nearest < fullDistance) desiredDistance = Mathf.Max(nearest, _minDistance);

        if (_debugDraw)
        {
            // Green = clear sightline; Red = blocked. Watch this line in the Scene view during Play (Gizmos on).
            if (hitCollider != null)
            {
                Debug.DrawLine(pivot, pivot + dir * nearest, Color.red);
                Debug.DrawLine(pivot + dir * nearest, transform.position, Color.yellow);
                Debug.Log($"[CameraWallCollision] HIT '{hitCollider.name}' at {nearest:F2}m (full {fullDistance:F2}m)", hitCollider);
            }
            else
            {
                Debug.DrawLine(pivot, transform.position, Color.green); // nothing in the way
            }
        }

        if (_currentDistance < 0f) _currentDistance = desiredDistance; // first frame
        // Snap inward instantly (never reveal the outside even for one frame), ease back out smoothly.
        if (desiredDistance < _currentDistance)
            _currentDistance = desiredDistance;
        else
            _currentDistance = Mathf.SmoothDamp(
                _currentDistance, desiredDistance, ref _distanceVelocity, _easeOutTime);

        transform.position = pivot + dir * _currentDistance;
        transform.LookAt(_target);
    }
}
