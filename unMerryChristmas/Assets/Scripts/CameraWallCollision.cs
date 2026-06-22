using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Self-contained isometric follow camera for an ENCLOSED room (e.g. the kitchen).
///
/// It REPLACES SmoothCameraFollow on this camera — it follows the target, cycles the 4 view
/// offsets on the "Toggle Camera" action (same R key), sets the FOV, AND keeps itself inside the
/// room. Because it is the ONLY script writing this camera's transform, nothing can overwrite the
/// result (which is what happened when SmoothCameraFollow and the wall logic both ran).
///
/// Staying inside the room (two guarantees, so it can never leak out):
///  1) SphereCast from the player along the view direction to the nearest wall/ceiling; the camera
///     stops there. Back-face queries are enabled for that cast so single-sided walls are detected
///     from the inside.
///  2) Hard clamp to the room's bounding box (from the blockers' MeshRenderers) — even if a cast
///     misses, the camera is forced back inside.
///
/// Setup: remove/disable SmoothCameraFollow on this camera. Assign Target (player) and Blockers
/// (kitchen + Roof). Offsets default to the backyard values.
/// </summary>
[DefaultExecutionOrder(100)]
public class CameraWallCollision : MonoBehaviour
{
    [Header("Follow (replaces SmoothCameraFollow)")]
    [Tooltip("The player to follow / look at.")]
    [SerializeField] private Transform _target;

    [Tooltip("The 4 view offsets, cycled with the Toggle Camera key. Defaults to the backyard values.")]
    [SerializeField] private Vector3[] _offsets =
    {
        new Vector3(-7.5f, 5f, -6f),
        new Vector3(-6f, 5f, 7.5f),
        new Vector3(7.5f, 5f, 6f),
        new Vector3(6f, 5f, -7.5f),
    };

    [SerializeField] private float _smoothTime = 0.05f;
    [SerializeField] private float _fieldOfView = 50f;

    [Header("Stay inside the room")]
    [Tooltip("Explicit INTERIOR volume the camera is hard-clamped inside. Add an empty GameObject with a " +
             "BoxCollider, size/position it to the kitchen interior, and assign it here. This is the reliable " +
             "boundary: it stops the camera leaking out through window/door openings (which have no wall " +
             "collider for the SphereCast to hit). If left unassigned, falls back to the blockers' bounding box.")]
    [SerializeField] private BoxCollider _interiorBounds;
    [Tooltip("Walls (kitchen) + ceiling (Roof). They stop the camera AND define the box it is kept inside.")]
    [SerializeField] private Transform[] _blockers;
    [SerializeField] private LayerMask _mask = ~0;
    [SerializeField] private float _radius = 0.25f;
    [SerializeField] private float _pivotLift = 0.5f;
    [SerializeField] private float _inset = 0.3f;
    [SerializeField] private float _easeOutTime = 0.18f;
    [SerializeField] private bool _debugDraw = true;

    private Camera _cam;
    private int _offsetIndex;
    private Vector3 _smoothedOffset;
    private Vector3 _offsetVelocity;
    private float _currentDistance = -1f;
    private float _distanceVelocity;
    private readonly RaycastHit[] _hits = new RaycastHit[32];
    private Bounds _box;
    private bool _hasBox;
    private InputAction _toggle;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null) _cam.fieldOfView = _fieldOfView;

        // Guarantee we are the ONLY thing driving this camera: turn off the follow script that
        // would otherwise fight us. (This camera uses THIS script instead of SmoothCameraFollow.)
        SmoothCameraFollow follow = GetComponent<SmoothCameraFollow>();
        if (follow != null)
        {
            follow.enabled = false;
            Debug.Log("[KitchenCamera] Disabled SmoothCameraFollow so this script is the sole camera driver.");
        }

        _smoothedOffset = (_offsets != null && _offsets.Length > 0) ? _offsets[0] : Vector3.zero;
        BuildBox();
    }

    private void OnEnable()
    {
        // Listen to the same shared "Toggle Camera" action the Controller uses, so R still cycles
        // views even though SmoothCameraFollow is gone.
        InputActionAsset asset = InputSystem.actions;
        if (asset != null) _toggle = asset.FindAction("Toggle Camera", throwIfNotFound: false);
        if (_toggle != null) { _toggle.started += OnToggle; _toggle.Enable(); }
    }

    private void OnDisable()
    {
        if (_toggle != null) _toggle.started -= OnToggle;
    }

    private void OnToggle(InputAction.CallbackContext _) => CycleView();

    public void CycleView()
    {
        if (_offsets == null || _offsets.Length == 0) return;
        _offsetIndex = (_offsetIndex + 1) % _offsets.Length;
        _offsetVelocity = Vector3.zero;
    }

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

        // 1) Follow: smooth toward the active offset.
        Vector3 activeOffset = (_offsets != null && _offsets.Length > 0) ? _offsets[_offsetIndex] : Vector3.zero;
        _smoothedOffset = Vector3.SmoothDamp(_smoothedOffset, activeOffset, ref _offsetVelocity, _smoothTime);

        Vector3 lookAt = _target.position;
        Vector3 toCam = _smoothedOffset;
        float fullDistance = toCam.magnitude;
        if (fullDistance < 0.001f) { transform.position = lookAt; transform.LookAt(lookAt); return; }
        Vector3 dir = toCam / fullDistance;
        Vector3 castFrom = lookAt + Vector3.up * _pivotLift;

        // 2) Cast to the nearest wall/ceiling (back faces enabled so single-sided walls register).
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
        if (targetDistance < _currentDistance) _currentDistance = targetDistance;
        else _currentDistance = Mathf.SmoothDamp(_currentDistance, targetDistance, ref _distanceVelocity, _easeOutTime);

        Vector3 unclampedPos = lookAt + dir * _currentDistance;
        Vector3 pos = unclampedPos;

        // 3) Hard safety net: never outside the room. Prefer the explicit interior volume (it is the
        //    only reliable boundary — it also blocks leaks through window/door OPENINGS, which the
        //    SphereCast passes straight through because an opening has no wall collider). The blockers'
        //    bounding box is only a fallback when no interior volume is assigned.
        string clampSrc = "none";
        if (_interiorBounds != null)
        {
            Bounds ib = _interiorBounds.bounds;          // world-space AABB of the assigned interior box
            Vector3 mn = ib.min + Vector3.one * _inset;
            Vector3 mx = ib.max - Vector3.one * _inset;
            pos.x = Mathf.Clamp(pos.x, Mathf.Min(mn.x, mx.x), Mathf.Max(mn.x, mx.x));
            pos.y = Mathf.Clamp(pos.y, Mathf.Min(mn.y, mx.y), Mathf.Max(mn.y, mx.y));
            pos.z = Mathf.Clamp(pos.z, Mathf.Min(mn.z, mx.z), Mathf.Max(mn.z, mx.z));
            clampSrc = "interior";
        }
        else
        {
            if (!_hasBox) BuildBox();
            if (_hasBox)
            {
                Vector3 mn = _box.min + Vector3.one * _inset;
                Vector3 mx = _box.max - Vector3.one * _inset;
                pos.x = Mathf.Clamp(pos.x, Mathf.Min(mn.x, mx.x), Mathf.Max(mn.x, mx.x));
                pos.y = Mathf.Clamp(pos.y, Mathf.Min(mn.y, mx.y), Mathf.Max(mn.y, mx.y));
                pos.z = Mathf.Clamp(pos.z, Mathf.Min(mn.z, mx.z), Mathf.Max(mn.z, mx.z));
                clampSrc = "shell-box";
            }
        }

        transform.position = pos;
        transform.LookAt(lookAt);

        if (_debugDraw)
        {
            bool clampMoved = (pos - unclampedPos).sqrMagnitude > 1e-6f;
            Debug.Log($"[KitchenCamera] view {_offsetIndex} {(hitCollider != null ? "HIT '" + hitCollider.name + "' @" + nearest.ToString("F2") : "NO-HIT(cast missed -> full offset)")} " +
                      $"dist={_currentDistance:F2}/{fullDistance:F2} player=({lookAt.x:F1},{lookAt.y:F1},{lookAt.z:F1}) " +
                      $"want=({unclampedPos.x:F1},{unclampedPos.y:F1},{unclampedPos.z:F1}) " +
                      $"cam=({pos.x:F1},{pos.y:F1},{pos.z:F1}){(clampMoved ? " [CLAMPED:" + clampSrc + "]" : "")}");
        }
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
        // Green = the explicit interior volume the camera is clamped inside (the boundary that matters).
        if (_interiorBounds != null)
        {
            Bounds ib = _interiorBounds.bounds;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ib.center, ib.size - Vector3.one * _inset * 2f);
        }

        // Cyan = the fallback blockers' shell box (only used when no interior volume is assigned).
        if (!Application.isPlaying) BuildBox();
        if (_hasBox)
        {
            Gizmos.color = _interiorBounds != null ? new Color(0f, 1f, 1f, 0.25f) : Color.cyan;
            Gizmos.DrawWireCube(_box.center, _box.size - Vector3.one * _inset * 2f);
        }
    }
#endif
}
