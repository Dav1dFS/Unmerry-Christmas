using UnityEngine;

/// <summary>
/// Tracks Task 2 — Garden Chaos.
/// Completes when the patio table has been tipped AND both chairs have been toppled.
/// Wire _chairA and _chairB to the two patio chair GameObjects.
/// TippablePatioTable calls OnTableTipped() directly via its serialized reference to this tracker.
/// </summary>
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
