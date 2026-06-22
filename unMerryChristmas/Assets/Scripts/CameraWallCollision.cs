using UnityEngine;

/// <summary>
/// Stops an isometric follow camera at the first wall/ceiling between it and the player, so the
/// player can never see outside an enclosed room — while keeping the EXACT camera angle of the
/// open scenarios.
///
/// How it works: SmoothCameraFollow places the camera at target + offset and looks at the target.
/// This add-on casts from the target ALONG that same offset direction and, if it hits a wall or
/// the ceiling, moves the camera to the hit point. Because the camera stays on that ray and still
/// LookAt()s the target, the view direction (the "angle") is identical to the backyard — only the
/// distance shrinks, which pulls the camera inside the room.
///
/// Runs in LateUpdate with [DefaultExecutionOrder] so it executes ONCE per frame, right after
/// SmoothCameraFollow has placed the camera. (An earlier version ran in a render callback, which
/// fires multiple times per frame — e.g. the editor Camera Preview when the camera is selected —
/// causing it to read its own output and flicker. LateUpdate avoids that entirely.)
///
/// Requires the walls AND the ceiling to have Colliders, assigned to <see cref="_blockers"/>
/// (e.g. the kitchen mesh and the Roof mesh). Only those block the camera — props are ignored.
///
/// Self-contained and scene-scoped: SmoothCameraFollow is untouched, so other scenarios are
/// unaffected — only cameras carrying this component get the wall stop.
/// </summary>
[DefaultExecutionOrder(100)] // after SmoothCameraFollow (default order 0)
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

    [Tooltip("Raises the cast start off the floor so the sphere doesn't begin inside the floor collider. The camera angle is unaffected.")]
    [SerializeField] private float _pivotLift = 0.5f;

    [Tooltip("How quickly the camera eases back OUT when the wall/ceiling moves away. It snaps IN instantly so the outside is never revealed.")]
    [SerializeField] private float _easeOutTime = 0.18f;

    [Tooltip("Log/draw the cast each frame, for diagnosis. Turn off once it works.")]
    [SerializeField] private bool _debugDraw = true;

    private float _currentDistance = -1f;   // smoothed camera distance (<0 = uninitialized)
    private float _distanceVelocity;
    private readonly RaycastHit[] _hits = new RaycastHit[32];

    private void LateUpdate()
    {
        if (_target == null) return;

        // SmoothCameraFollow (order 0) has already set transform.position to target + offset this frame.
        Vector3 lookAt  = _target.position;
        Vector3 castFrom = lookAt + Vector3.up * _pivotLift;      // lift only the cast start off the floor
        Vector3 toCam   = transform.position - lookAt;           // full offset (backyard angle)
        float fullDistance = toCam.magnitude;
        if (fullDistance < 0.001f) return;

        Vector3 dir = toCam / fullDistance;                      // the backyard view direction — only shortened, never rotated

        // Nearest wall/ceiling along that ray.
        float nearest = fullDistance;
        Collider hitCollider = null;
        int n = Physics.SphereCastNonAlloc(castFrom, _radius, dir, _hits, fullDistance, _mask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
        {
            RaycastHit h = _hits[i];
            if (h.distance <= 0f) continue;                       // sphere already overlapping at the start
            if (!IsBlocker(h.collider.transform)) continue;       // only walls/ceiling, never the player or props
            if (h.distance < nearest) { nearest = h.distance; hitCollider = h.collider; }
        }

        float targetDistance = nearest; // = hit point, or the full offset if nothing was in the way

        if (_currentDistance < 0f) _currentDistance = targetDistance;     // first frame
        if (targetDistance < _currentDistance)
            _currentDistance = targetDistance;                            // snap IN instantly (never reveal outside)
        else
            _currentDistance = Mathf.SmoothDamp(                          // ease back OUT smoothly
                _currentDistance, targetDistance, ref _distanceVelocity, _easeOutTime);

        transform.position = lookAt + dir * _currentDistance;             // same angle, distance = hit point
        transform.LookAt(lookAt);

        if (_debugDraw)
        {
            Debug.DrawLine(castFrom, castFrom + dir * _currentDistance, hitCollider != null ? Color.red : Color.green);
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
