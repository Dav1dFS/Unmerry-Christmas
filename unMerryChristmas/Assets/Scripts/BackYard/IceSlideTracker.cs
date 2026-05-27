using UnityEngine;
public class IceSlideTracker : MonoBehaviour
{
    [SerializeField] private IcePatch   _icePatch;
    [SerializeField] private CompostBin _compostBin;
    [SerializeField] private Controller _playerController;

    private bool _completed;

    private void OnEnable()  => _compostBin.OnOpenedByCollision += HandleCompostCollision;
    private void OnDisable() => _compostBin.OnOpenedByCollision -= HandleCompostCollision;

    private void HandleCompostCollision()
    {
        if (_completed) return;
        if (!_icePatch.IsActive) return;
        if (!_playerController.IsRolling) return;

        _completed = true;
        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.IceSlide);
    }
}
