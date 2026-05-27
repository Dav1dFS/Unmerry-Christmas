using UnityEngine;
public class BreakableBirdbath : MonoBehaviour, IBreakable
{
    [SerializeField] private GameObject _intactVisual;
    [SerializeField] private GameObject _shatteredBasinVisual;  // basin shards mesh/particles
    [SerializeField] private GameObject _fullDestroyedVisual;   // base rubble (optional explosive stage)

    private bool _basinBroken;
    private bool _completed;

    public void Break()
    {
        if (_basinBroken) return;
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing))
        {
            Debug.Log("[BreakableBirdbath] Throwing not yet unlocked.");
            return;
        }

        _basinBroken = true;
        if (_intactVisual        != null) _intactVisual.SetActive(false);
        if (_shatteredBasinVisual != null) _shatteredBasinVisual.SetActive(true);

        if (!_completed)
        {
            _completed = true;
            BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.BirdbathDemolition);
        }
    }
    public void FullDestroy()
    {
        if (!_basinBroken) Break();
        if (_shatteredBasinVisual != null) _shatteredBasinVisual.SetActive(false);
        if (_fullDestroyedVisual  != null) _fullDestroyedVisual.SetActive(true);
    }
}
