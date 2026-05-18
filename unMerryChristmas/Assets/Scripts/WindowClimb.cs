using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindowClimb : MonoBehaviour
{
    [SerializeField] private Transform _benchTransform;
    [SerializeField] private Transform _windowWallPoint;
    [SerializeField] private float _requiredDistance = 1.8f;
    [SerializeField] private float _freezeDuration = 0.8f;

    [SerializeField] private string _targetScene = "KitchenScenario";
    [SerializeField] private string _entryPointId = "from_backyard_window";

    private bool _completed;

    private void OnTriggerEnter(Collider other)
    {
        if (_completed) return;
        if (!other.CompareTag("Player")) return;

        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Rolling))
        {
            Debug.Log("[WindowClimb] Cannot climb: Rolling ability not unlocked.");
            return;
        }

        float benchDistance = Vector3.Distance(_benchTransform.position, _windowWallPoint.position);
        if (benchDistance > _requiredDistance)
        {
            Debug.Log("[WindowClimb] Cannot climb: bench is not close enough to the window.");
            return;
        }

        StartCoroutine(DoClimb());
    }

    private IEnumerator DoClimb()
    {
        _completed = true;

        PlayerFreezeManager.Instance.Freeze();
        yield return new WaitForSeconds(_freezeDuration);
        PlayerFreezeManager.Instance.UnFreeze();

        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.MainClimbWindow);

        SceneEntryPoint.RequestedEntryId = _entryPointId;
        SceneManager.LoadScene(_targetScene);
    }
}
