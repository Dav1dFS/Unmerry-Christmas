using System;
using UnityEngine;
public class Padlock : MonoBehaviour, IBreakable
{
    [SerializeField] private ShedDoor   _shedDoor;
    [SerializeField] private GameObject _intactVisual;
    [SerializeField] private GameObject _brokenVisual;

    public event Action OnBroken;
    private bool _broken;

    public void Break()
    {
        if (_broken) return;
        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Throwing))
        {
            Debug.Log("[Padlock] Throwing not yet unlocked.");
            return;
        }

        _broken = true;
        if (_intactVisual != null) _intactVisual.SetActive(false);
        if (_brokenVisual != null) _brokenVisual.SetActive(true);

        _shedDoor?.Unlock();
        OnBroken?.Invoke();
    }
}
