using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string _targetScene;

    [Tooltip("Optional: spawn point ID to use in the target scene. " +
             "The target scene's SceneEntryPoint with matching ID will position the player.")]
    [SerializeField] private string _entryPointId = "";

    private Collider _collider;

    // When the player spawns *inside* this trigger (e.g. it sits on the entry
    // point you arrive at), firing immediately would teleport them straight back.
    // Disarm until the player has left the trigger at least once.
    private bool _armed = true;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnEnable() => StartCoroutine(ArmAfterSpawn());

    // Wait one frame so SceneBootstrap has positioned the player, then disarm if
    // the player spawned overlapping us. OnTriggerExit re-arms on the way out.
    private IEnumerator ArmAfterSpawn()
    {
        yield return null;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && _collider.bounds.Contains(player.transform.position))
            _armed = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _armed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_armed || !other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(_entryPointId))
            SceneEntryPoint.RequestedEntryId = _entryPointId;

        SceneManager.LoadScene(_targetScene);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
        var col = GetComponent<Collider>();
        if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        UnityEditor.Handles.Label(transform.position + Vector3.up,
            $"→ {_targetScene}" + (_entryPointId != "" ? $" [{_entryPointId}]" : ""));
    }
#endif
}
