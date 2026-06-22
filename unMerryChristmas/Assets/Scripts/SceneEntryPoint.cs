using UnityEngine;
public class SceneEntryPoint : MonoBehaviour
{
    // Set by SceneTransitionTrigger before loading; consumed on arrival.
    public static string RequestedEntryId { get; set; } = "";

    [SerializeField] private string _entryId = "default";
    [SerializeField] private bool   _isDefault = false;

    private static SceneEntryPoint _defaultPoint;

    private void OnEnable()
    {
        if (_isDefault) _defaultPoint = this;
    }
    public static void PositionPlayer(Transform player)
    {
        if (player == null) return;

        SceneEntryPoint[] allPoints = FindObjectsByType<SceneEntryPoint>(FindObjectsSortMode.None);
        SceneEntryPoint target = null;

        if (!string.IsNullOrEmpty(RequestedEntryId))
        {
            foreach (var pt in allPoints)
            {
                if (pt._entryId == RequestedEntryId) { target = pt; break; }
            }
        }

        if (target == null) target = _defaultPoint;

        if (target != null)
        {
            Transform t = target.transform;
            player.SetPositionAndRotation(t.position, t.rotation);

            // The player is Rigidbody-driven; teleporting the transform alone can
            // leave stale velocity (a slide on arrival) and the physics body lagging
            // a frame behind. Move the body too and clear any carried-over motion.
            if (player.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.position        = t.position;
                rb.rotation        = t.rotation;
                rb.linearVelocity  = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Physics.SyncTransforms();
        }

        RequestedEntryId = "";   // consume
        _defaultPoint    = null; // reset for next scene
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = _isDefault ? Color.green : Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawRay(transform.position, transform.forward * 0.8f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
            _isDefault ? $"[default] {_entryId}" : _entryId);
    }
#endif
}
