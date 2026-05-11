using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string _targetScene;

    [Tooltip("Optional: spawn point ID to use in the target scene. " +
             "The target scene's SceneEntryPoint with matching ID will position the player.")]
    [SerializeField] private string _entryPointId = "";

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

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
