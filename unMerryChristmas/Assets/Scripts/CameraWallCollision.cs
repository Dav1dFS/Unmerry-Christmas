using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Stops an isometric follow camera at the first wall/ceiling between it and the player, so the
/// player can never see outside an enclosed room — while keeping the EXACT camera angle of the
/// open scenarios.
///
/// How it works: the follow camera (SmoothCameraFollow) places the camera at target + offset and
/// looks at the target. This add-on casts from the target ALONG that same offset direction and,
/// if it hits a wall or the ceiling, moves the camera to the hit point. Because the camera stays
/// on that ray and still LookAt()s the target, the view direction (the "angle") is identical to
/// the backyard — only the distance shrinks, which pulls the camera inside the room.
///
/// Requires the walls AND the ceiling to have Colliders, assigned to <see cref="_blockers"/>
/// (e.g. the kitchen mesh and the Roof mesh). Only those block the camera — props are ignored.
///
/// Runs in RenderPipelineManager.beginCameraRendering (URP's equivalent of the Built-in
/// Camera.onPreCull, which does NOT fire under URP) — after every LateUpdate (so the follow
/// camera has placed itself) but before the camera renders, so nothing can overwrite it.
///
/// Self-contained and scene-scoped: SmoothCameraFollow is untouched, so other scenarios are
/// unaffected — only cameras carrying this component get the wall stop.
/// </summary>
public class CameraWallCollision : MonoBehaviour
{
    [Tooltip("What the camera follows / looks at. Assign the same target SmoothCameraFollow uses (the player).")]
    [SerializeField] private Transform _target;

    [Tooltip("Only colliders under these objects stop the camera. Assign the walls (kitchen) and the ceiling (Roof). Props are ignored.")]
    [SerializeField] private Transform[] _blockers;

    [Tooltip("Broad-phase layers to test. Leave as Everything; the Blockers list does the real filtering.")]
    [SerializeField] private LayerMask _mask = ~0;

    [Tooltip("Camera 'thickness' — how far off the surface it stops, so it never clips through. Increase if the lens still pokes through at corners.")]
    [SerializeField] private float _radius = 0.25f;

    [Tooltip("How quickly the camera eases back OUT when the wall/ceiling moves away. It snaps IN instantly so the outside is never revealed.")]
    [SerializeField] private float _easeOutTime = 0.18f;

    [Tooltip("Log/draw the cast each frame, for diagnosis. Turn off once it works.")]
    [SerializeField] private bool _debugDraw = true;

    private Camera _cam;
    private float _currentDistance = -1f;   // smoothed camera distance (<0 = uninitialized)
    private float _distanceVelocity;
    private readonly RaycastHit[] _hits = new RaycastHit[32];

    private void Awake() => _cam = GetComponent<Camera>();

    private void OnEnable()  => RenderPipelineManager.beginCameraRendering += ApplyWallStop;
    private void OnDisable() => RenderPipelineManager.beginCameraRendering -= ApplyWallStop;

    // URP render callback: after all LateUpdates, just before this camera renders.
    private void ApplyWallStop(ScriptableRenderContext context, Camera cam)
    {
        if (cam != _cam || _target == null) return;

        Vector3 pivot   = _target.position;          // SmoothCameraFollow looks here
        Vector3 desired = transform.position;        // ...and placed the camera here (full offset, backyard angle)
        Vector3 toCam   = desired - pivot;
        float fullDistance = toCam.magnitude;
        if (fullDistance < 0.001f) return;

        Vector3 dir = toCam / fullDistance;          // the backyard view direction — never changed, only shortened

        // Find the nearest wall/ceiling along that ray.
        float nearest = fullDistance;
        Collider hitCollider = null;
        int n = Physics.SphereCastNonAlloc(pivot, _radius, dir, _hits, fullDistance, _mask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            RaycastHit h = _hits[i];
            if (h.distance <= 0f) continue;           // sphere already overlapping a collider at the start
            if (!IsBlocker(h.collider.transform)) continue; // only walls/ceiling, never the player or props
            if (h.distance < nearest) { nearest = h.distance; hitCollider = h.collider; }
        }

        float targetDistance = nearest; // = hit point, or the full offset if nothing was in the way

        if (_currentDistance < 0f) _currentDistance = targetDistance;     // first frame
        if (targetDistance < _currentDistance)
            _currentDistance = targetDistance;                            // snap IN instantly (never reveal outside)
        else
            _currentDistance = Mathf.SmoothDamp(                          // ease back OUT smoothly
                _currentDistance, targetDistance, ref _distanceVelocity, _easeOutTime);

        transform.position = pivot + dir * _currentDistance;              // same angle, distance = hit point
        transform.LookAt(_target);

        if (_debugDraw)
        {
            Debug.DrawLine(pivot, transform.position, hitCollider != null ? Color.red : Color.green);
            Debug.Log($"[CameraWallCollision] {(hitCollider != null ? "HIT '" + hitCollider.name + "'" : "no hit")} " +
                      $"dist={_currentDistance:F2} (full {fullDistance:F2})");
        }
    }

    private bool IsBlocker(Transform t)
    {
        if (_blockers == null) return false;
        for (int i = 0; i < _blockers.Length; i++)
            if (_blockers[i] != null && t.IsChildOf(_blockers[i])) return true;
        return false;
    }
}
