using UnityEngine;
public class IceSlideTracker : MonoBehaviour
{
    [SerializeField] private IcePatch   _icePatch;
    [SerializeField] private CompostBin _compostBin;
    [SerializeField] private Controller _playerController;

    private bool _completed;

    private void OnEnable()
    {
        if (_compostBin != null) _compostBin.OnOpenedByCollision += HandleCompostCollision;
        else Debug.LogWarning("[IceSlideTracker] _compostBin is not assigned — wire it in the Inspector or run the BackYard Auto-Wire tool.", this);
    }

    private void OnDisable()
    {
        if (_compostBin != null) _compostBin.OnOpenedByCollision -= HandleCompostCollision;
    }

    private void HandleCompostCollision()
    {
        if (_completed) return;
        if (!_icePatch.IsActive) return;
        if (!_playerController.IsRolling) return;

        _completed = true;
        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.IceSlide);
    }
}
