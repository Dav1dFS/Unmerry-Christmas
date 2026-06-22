using UnityEngine;

/// <summary>
/// Keeps the isometric follow camera INSIDE an enclosed room, parked at the wall/ceiling between
/// it and the player (a close, "cramped" inside view), preserving the backyard view angle.
///
/// Two mechanisms, so it can never leak outside:
///  1) A SphereCast from the player along the backyard offset direction finds the nearest wall /
///     ceiling and stops the camera there. Back-face queries are enabled for that cast, so the
///     walls are detected even if their meshes are single-sided with outward normals (otherwise a
///     ray from inside passes straight through and the camera escapes).
///  2) A hard clamp to the room's bounding box (built from the blockers' MeshRenderers) — a
///     guaranteed safety net: even if a cast ever misses (an opening, odd geometry), the camera
///     is forced back inside the box.
///
/// Runs in LateUpdate with [DefaultExecutionOrder] so it executes once per frame, after
/// SmoothCameraFollow has placed the camera.
///
/// Self-contained and scene-scoped: SmoothCameraFollow is untouched; only cameras carrying this
/// component are affected.
/// </summary>
[DefaultExecutionOrder(100)] // after SmoothCameraFollow (default order 0)
public class CameraWallCollision : MonoBehaviour
{
    [Tooltip("What the camera follows / looks at. Assign the player.")]
    [SerializeField] private Transform _target;

    [Tooltip("The walls (kitchen) and ceiling (Roof). These stop the camera AND define the room box it is kept inside.")]
    [SerializeField] private Transform[] _blockers;

    [Tooltip("Broad-phase layers. Leave as Everything; the Blockers list does the filtering.")]
    [SerializeField] private LayerMask _mask = ~0;

    [Tooltip("Camera 'thickness' kept off surfaces.")]
    [SerializeField] private float _radius = 0.25f;

    [Tooltip("Raises the cast start off the floor so the sphere doesn't begin inside the floor collider.")]
    [SerializeField] private float _pivotLift = 0.5f;

    [Tooltip("Safety net: keep the camera at least this far inside the room's bounding box.")]
    [SerializeField] private float _inset = 0.3f;

    [Tooltip("How quickly the camera eases back out when the wall/ceiling moves away.")]
    [SerializeField] private float _easeOutTime = 0.18f;

    [Tooltip("Log the result each frame, for diagnosis. Turn off once it works.")]
    [SerializeField] private bool _debugDraw = true;

    private float _currentDistance = -1f;
    private float _distanceVelocity;
    private readonly RaycastHit[] _hits = new RaycastHit[32];
    private Bounds _box;
    private bool _hasBox;

    private void Awake() => BuildBox();

    // Box from the blockers' MeshRenderers only (MeshRenderer excludes the player's SkinnedMeshRenderer,
    // so the player can't inflate/jitter the box). Static room → compute once.
    private void BuildBox()
    {
        _hasBox = false;
        if (_blockers == null) return;

        bool any = false;
        Bounds b = default;
        foreach (Transform bl in _blockers)
        {
            if (bl == null) continue;
            foreach (MeshRenderer r in bl.GetComponentsInChildren<MeshRenderer>())
            {
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
        }
        if (any) { _box = b; _hasBox = true; }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 lookAt   = _target.position;
        Vector3 castFrom = lookAt + Vector3.up * _pivotLift;
        Vector3 toCam    = transform.position - lookAt;          // full backyard offset
        float fullDistance = toCam.magnitude;
        if (fullDistance < 0.001f) return;
        Vector3 dir = toCam / fullDistance;                      // backyard angle — only shortened

        // Cast for the wall/ceiling, detecting back faces so single-sided walls are seen from inside.
        bool prevBackfaces = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true;
        int n = Physics.SphereCastNonAlloc(castFrom, _radius, dir, _hits, fullDistance, _mask, QueryTriggerInteraction.Ignore);
        Physics.queriesHitBackfaces = prevBackfaces;

        float nearest = fullDistance;
        Collider hitCollider = null;
        for (int i = 0; i < n; i++)
        {
            RaycastHit h = _hits[i];
            if (h.distance <= 0f) continue;
            if (!IsBlocker(h.collider.transform)) continue;
            if (h.distance < nearest) { nearest = h.distance; hitCollider = h.collider; }
        }
        float targetDistance = nearest;

        if (_currentDistance < 0f) _currentDistance = targetDistance;
        if (targetDistance < _currentDistance) _currentDistance = targetDistance;     // snap in instantly
        else _currentDistance = Mathf.SmoothDamp(_currentDistance, targetDistance, ref _distanceVelocity, _easeOutTime);

        Vector3 pos = lookAt + dir * _currentDistance;

        // Hard safety net: never allow the camera outside the room box.
        if (!_hasBox) BuildBox();
        if (_hasBox)
        {
            Vector3 mn = _box.min + Vector3.one * _inset;
            Vector3 mx = _box.max - Vector3.one * _inset;
            pos.x = Mathf.Clamp(pos.x, Mathf.Min(mn.x, mx.x), Mathf.Max(mn.x, mx.x));
            pos.y = Mathf.Clamp(pos.y, Mathf.Min(mn.y, mx.y), Mathf.Max(mn.y, mx.y));
            pos.z = Mathf.Clamp(pos.z, Mathf.Min(mn.z, mx.z), Mathf.Max(mn.z, mx.z));
        }

        transform.position = pos;
        transform.LookAt(lookAt);

        if (_debugDraw)
            Debug.Log($"[CameraWallCollision] {(hitCollider != null ? "HIT '" + hitCollider.name + "'" : "no hit")} " +
                      $"dist={_currentDistance:F2} cam=({pos.x:F1},{pos.y:F1},{pos.z:F1})" + (_hasBox ? "" : "  [NO BOX - assign Blockers!]"));
    }

    private bool IsBlocker(Transform t)
    {
        if (_blockers == null) return false;
        for (int i = 0; i < _blockers.Length; i++)
            if (_blockers[i] != null && t.IsChildOf(_blockers[i])) return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) BuildBox();
        if (!_hasBox) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_box.center, _box.size - Vector3.one * _inset * 2f);
    }
#endif
}
