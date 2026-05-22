using System;
using UnityEngine;
public class MovingBox : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _closedVisual;
    [SerializeField] private GameObject _openVisual;   // pre-arranged spilled contents mesh
    [SerializeField] private GameObject _snowCover;    // optional snow pile on top of box
    [SerializeField] private GameObject _rewardPrefab; // StuffedBear (only on last box)
    [SerializeField] private Transform _rewardSpawnPoint; // where to spawn reward (defaults to box position)

    public event Action OnOpened;
    private bool _isOpen;

    public void Interact()
    {
        if (_isOpen) return;
        Open();
    }

    public string GetHintText() => "E — Open Box";

    private void Open()
    {
        _isOpen = true;

        // Show opening animation briefly, then disappear
        if (_snowCover   != null) _snowCover.SetActive(false);
        if (_closedVisual != null) _closedVisual.SetActive(false);
        if (_openVisual   != null) _openVisual.SetActive(true);

        // Spawn reward if this box has one configured
        if (_rewardPrefab != null)
        {
            Vector3 spawnPos = _rewardSpawnPoint != null ? _rewardSpawnPoint.position : transform.position;
            Instantiate(_rewardPrefab, spawnPos, Quaternion.identity);
        }

        // Disable entire box after brief delay (or immediately if no visual)
        gameObject.SetActive(false);

        OnOpened?.Invoke();
    }
}
