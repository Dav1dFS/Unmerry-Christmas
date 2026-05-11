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
        _chairA.OnToppled += () => { _chairsDown++; TryComplete(); };
        _chairB.OnToppled += () => { _chairsDown++; TryComplete(); };
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
        if (!_tableDown || _chairsDown < 2) return;
        _completed = true;
        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.GardenChaos);
    }
}
