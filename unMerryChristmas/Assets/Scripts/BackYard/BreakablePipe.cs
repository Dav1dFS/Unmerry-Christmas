using System;
using UnityEngine;
public class BreakablePipe : MonoBehaviour, IBreakable
{
    [SerializeField] private GameObject _intactVisual;
    [SerializeField] private GameObject _burstVisual;   // water spray particles or burst mesh
    [SerializeField] private IcePatch   _icePatch;

    public event Action OnBurst;
    private bool _broken;

    public void Break()
    {
        if (_broken) return;
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing))
            return;

        _broken = true;
        if (_intactVisual != null) _intactVisual.SetActive(false);
        if (_burstVisual  != null) _burstVisual.SetActive(true);

        _icePatch.Activate();
        OnBurst?.Invoke();

        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.BurstPipe);

        // Unlock IceSlide now that the ice exists
        BackYardTaskTracker.UnlockTask(BackYardTaskTracker.TaskId.IceSlide);
    }
}
