using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class WindowClimb : MonoBehaviour
{
    [SerializeField] private string _targetScene   = "KitchenScenario";
    [SerializeField] private string _entryPointId  = "from_backyard_window";

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.MainClimbWindow);

        SceneEntryPoint.RequestedEntryId = _entryPointId;
        SceneManager.LoadScene(_targetScene);
    }
}
