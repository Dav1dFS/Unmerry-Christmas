using System;
using UnityEngine;
public class MovingBox : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _closedVisual;
    [SerializeField] private GameObject _openVisual;   // pre-arranged spilled contents mesh
    [SerializeField] private GameObject _snowCover;    // optional snow pile on top of box
    [SerializeField] private GameObject _rewardObject; // StuffedBear scene object (only on last box, starts inactive)

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

        // Reveal reward by enabling its renderers and colliders
        if (_rewardObject != null)
        {
            foreach (var r in _rewardObject.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;
            foreach (var c in _rewardObject.GetComponentsInChildren<Collider>(true))
                c.enabled = true;
        }

        // Disable entire box after brief delay (or immediately if no visual)
        gameObject.SetActive(false);

        OnOpened?.Invoke();
    }
}
