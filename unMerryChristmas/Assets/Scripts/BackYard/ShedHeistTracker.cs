using UnityEngine;
public class ShedHeistTracker : MonoBehaviour
{
    public static ShedHeistTracker Instance { get; private set; }

    [SerializeField] private int _totalShelves = 3;

    private int  _cleared;
    private bool _completed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ReportShelfCleared()
    {
        if (_completed) return;
        _cleared++;
        if (_cleared >= _totalShelves)
        {
            _completed = true;
            BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.ShedHeist);
        }
    }
}
