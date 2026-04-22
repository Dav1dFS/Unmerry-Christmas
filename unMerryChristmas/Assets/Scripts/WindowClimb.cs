using System.Collections;
using UnityEngine;

public class WindowClimb : MonoBehaviour
{
    [SerializeField] private Transform _benchTransform;
    [SerializeField] private Transform _windowWallPoint;
    [SerializeField] private float _requiredDistance = 1.8f;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private float _freezeDuration = 0.8f;

    private bool _completed;

    private void OnTriggerEnter(Collider other)
    {
        if (_completed) return;
        if (!other.CompareTag("Player")) return;

        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Rolling))
        {
            // TODO: replace with HUD hint when HUD system is implemented
            Debug.Log("[WindowClimb] Cannot climb: Rolling ability not unlocked.");
            return;
        }

        float benchDistance = Vector3.Distance(_benchTransform.position, _windowWallPoint.position);
        if (benchDistance > _requiredDistance)
        {
            // TODO: replace with HUD hint when HUD system is implemented
            Debug.Log("[WindowClimb] Cannot climb: bench is not close enough to the window.");
            return;
        }

        StartCoroutine(DoClimb(other.transform));
    }

    private IEnumerator DoClimb(Transform player)
    {
        _completed = true;

        PlayerFreezeManager.Instance.Freeze();
        yield return new WaitForSeconds(_freezeDuration);

        player.position = _exitPoint.position;
        player.rotation = _exitPoint.rotation;

        PlayerFreezeManager.Instance.UnFreeze();

        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.MainClimbWindow);
        Debug.Log("[WindowClimb] Main task completed. Player moved to kitchen.");
    }
}
