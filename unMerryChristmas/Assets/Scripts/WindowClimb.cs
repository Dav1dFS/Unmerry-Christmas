using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class WindowClimb : MonoBehaviour
{
    [SerializeField] private string _targetScene   = "KitchenScenario";
    [SerializeField] private string _entryPointId  = "from_backyard_window";

    private Collider _collider;

    // Disarm if the player spawns inside the trigger so arriving back here from
    // the Kitchen doesn't immediately climb through again. Re-arms on exit.
    private bool _armed = true;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnEnable() => StartCoroutine(ArmAfterSpawn());

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

        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.MainClimbWindow);

        SceneEntryPoint.RequestedEntryId = _entryPointId;
        SceneManager.LoadScene(_targetScene);
    }
}
