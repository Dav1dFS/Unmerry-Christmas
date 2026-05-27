using UnityEngine;
public class GardenChaosTracker : MonoBehaviour
{
    [SerializeField] private TopplableChair _chairA;
    [SerializeField] private TopplableChair _chairB;

    private bool _tableDown;
    private int  _chairsDown;
    private bool _completed;

    private void OnEnable()
    {
        if (_chairA != null) _chairA.OnToppled += () => { _chairsDown++; TryComplete(); };
        if (_chairB != null) _chairB.OnToppled += () => { _chairsDown++; TryComplete(); };
    }

    private void OnDisable()
    {
        if (_chairA != null) _chairA.OnToppled -= () => { _chairsDown++; TryComplete(); };
        if (_chairB != null) _chairB.OnToppled -= () => { _chairsDown++; TryComplete(); };
    }

    // Called by TippablePatioTable when the table tips over
    public void OnTableTipped()
    {
        _tableDown = true;
        TryComplete();
    }

    private void TryComplete()
    {
        if (_completed) return;
        // Complete when table is down (chairs are optional)
        if (!_tableDown) return;
        _completed = true;
        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.GardenChaos);
    }
}
