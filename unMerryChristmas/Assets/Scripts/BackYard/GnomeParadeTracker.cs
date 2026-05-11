using UnityEngine;
public class GnomeParadeTracker : MonoBehaviour
{
    [SerializeField] private GnomePlacementZone[] _zones;

    private int _filled;
    private bool _completed;

    private void OnEnable()
    {
        foreach (var zone in _zones)
            zone.OnGnomePlaced += OnZoneFilled;
    }

    private void OnDisable()
    {
        foreach (var zone in _zones)
            zone.OnGnomePlaced -= OnZoneFilled;
    }

    private void OnZoneFilled()
    {
        if (_completed) return;
        _filled++;
        if (_filled >= _zones.Length)
        {
            _completed = true;
            BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.GnomeParade);
        }
    }
}
