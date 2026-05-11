using System;
using UnityEngine;
public class MovingBox : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _closedVisual;
    [SerializeField] private GameObject _openVisual;   // pre-arranged spilled contents mesh
    [SerializeField] private GameObject _snowCover;    // optional snow pile on top of box

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
        if (_snowCover   != null) _snowCover.SetActive(false);
        if (_closedVisual != null) _closedVisual.SetActive(false);
        if (_openVisual   != null) _openVisual.SetActive(true);

        OnOpened?.Invoke();
    }
}
