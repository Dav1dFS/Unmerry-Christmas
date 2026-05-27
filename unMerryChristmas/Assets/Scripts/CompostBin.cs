using System;
using UnityEngine;

public class CompostBin : MonoBehaviour, IInteractable
{
    [SerializeField] private DrawingPageCollectable _drawingPage;
    [SerializeField] private Transform _lidTransform;
    [SerializeField] private float _openAngle = 90f;

    // Fired when a physics collision opens the bin (ice slide detection)
    public event Action OnOpenedByCollision;

    private bool _isOpen;

    // Called by Controller when player presses Interact near this object.
    // Requires: this GameObject is on the Pickup layer.
    public void Interact()
    {
        if (_isOpen) return;
        Open();
    }

    public string GetHintText() => "E — Open Bin";

    // Called when the ice-slide (Task 7) sends the elf into the bin.
    private void OnCollisionEnter(Collision collision)
    {
        if (_isOpen) return;
        Open();
        OnOpenedByCollision?.Invoke();
    }

    private void Open()
    {
        _isOpen = true;

        if (_lidTransform != null)
            _lidTransform.localRotation = Quaternion.Euler(_openAngle, 0f, 0f);

        if (_drawingPage != null)
            _drawingPage.gameObject.SetActive(true);
    }
}
